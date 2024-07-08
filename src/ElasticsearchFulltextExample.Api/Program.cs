// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Serilog.Filters;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using OpenFga.Sdk.Client;
using GitClub.Services;
using GitClub.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Claims;
using System.Threading.RateLimiting;
using GitClub.Infrastructure.Errors.Translators;
using GitClub.Infrastructure.Errors;
using Microsoft.AspNetCore.Authentication.Cookies;
using NodaTime.Serialization.SystemTextJson;
using NodaTime;
using GitClub.Infrastructure.Mvc;
using GitClub.Database.Models;
using Microsoft.AspNetCore.Authentication;
using GitClub.Infrastructure.Outbox.Consumer;
using ElasticsearchFulltextExample.Api.Constants;
using ElasticsearchFulltextExample.Api.Infrastructure.Mvc;
using ElasticsearchFulltextExample.Database.Model;
using ElasticsearchFulltextExample.Api.Infrastructure.Errors;
using ElasticsearchFulltextExample.Api.Infrastructure.Errors.Translators;
using ElasticsearchFulltextExample.Api.Infrastructure.Authentication;
using ElasticsearchFulltextExample.Api.Hosting;
using ElasticsearchFulltextExample.Api.Configuration;
using ElasticsearchFulltextExample.Database;
using ElasticsearchFulltextExample.Api.Services;
using ElasticsearchFulltextExample.Api.Infrastructure.Tesseract;

// We will log to %LocalAppData%/GitClub to store the Logs, so it doesn't need to be configured 
// to a different path, when you run it on your machine.
string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GitClub");

// We are writing with RollingFileAppender using a daily rotation, and we want to have the filename as 
// as "GitClub-{Date}.log", the date will be set by Serilog automagically.
string logFilePath = Path.Combine(logDirectory, "GitClub-.log");

// Configure the Serilog Logger. This Serilog Logger will be passed 
// to the Microsoft.Extensions.Logging LoggingBuilder using the 
// LoggingBuilder#AddSerilog(...) extension.
Log.Logger = new LoggerConfiguration()
    .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware"))
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

    // Logging
    builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

    // Database
    builder.Services.AddSingleton<NpgsqlDataSource>((sp) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("ApplicationDatabase");

        if (connectionString == null)
        {
            throw new InvalidOperationException("No ConnectionString named 'ApplicationDatabase' was found");
        }

        // Since version 7.0, NpgsqlDataSource is the recommended way to use Npgsql. When using NpsgqlDataSource,
        // NodaTime currently has to be configured twice - once at the EF level, and once at the underlying ADO.NET
        // level (there are plans to improve this):
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        // Call UseNodaTime() when building your data source:
        dataSourceBuilder.UseNodaTime();

        return dataSourceBuilder.Build();
    });

    // Database
    builder.Services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
    {
        var dataSource = sp.GetRequiredService<NpgsqlDataSource>();

        // Then, when configuring EF Core with UseNpgsql(), call UseNodaTime() there as well:
        options
            .EnableSensitiveDataLogging()
            .UseNpgsql(dataSource, options => options.UseNodaTime());
    });

    // Configures the Postgres Outbox Event Settings.
    builder.Services.AddSingleton<OutboxEventConsumer>();

    builder.Services.Configure<PostgresOutboxEventProcessorOptions>(o =>
    {
        var connectionString = builder.Configuration.GetConnectionString("ApplicationDatabase")!;

        o.ConnectionString = connectionString;
        o.PublicationName = "outbox_pub";
        o.ReplicationSlotName = "outbox_slot";
        o.OutboxEventSchemaName = "fts";
        o.OutboxEventTableName = "outbox_event";
    });

    builder.Services.AddHostedService<PostgresOutboxEventProcessor>();

    // Authentication
    builder.Services.AddScoped<CurrentUser>();
    builder.Services.AddScoped<IClaimsTransformation, CurrentUserClaimsTransformation>();

    // CORS
    builder.Services.AddCors(options =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("AllowedOrigins")
            .Get<string[]>();

        if (allowedOrigins == null)
        {
            throw new InvalidOperationException("AllowedOrigins is missing in the appsettings.json");
        }

        options.AddPolicy("CorsPolicy", builder => builder
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
    });

    // Add Exception Handling
    builder.Services.AddSingleton<IExceptionTranslator, DefaultExceptionTranslator>();
    builder.Services.AddSingleton<IExceptionTranslator, ApplicationErrorExceptionTranslator>();
    builder.Services.AddSingleton<IExceptionTranslator, InvalidModelStateExceptionTranslator>();

    builder.Services.Configure<ExceptionToApplicationErrorMapperOptions>(o =>
    {
        o.IncludeExceptionDetails = builder.Environment.IsDevelopment() || builder.Environment.IsStaging();
    });

    builder.Services.AddSingleton<ExceptionToApplicationErrorMapper>();

    // Application Services
    builder.Services.AddSingleton<TesseractExecutor>();
    builder.Services.AddSingleton<ElasticsearchService>();

    // Route Constraints
    builder.Services.Configure<RouteOptions>(options =>
    {
        options.ConstraintMap.Add("JobStatusEnum", typeof(EnumRouteConstraint<JobStatusEnum>));
    });

    // Controllers
    builder.Services
        .AddControllers()
        .AddJsonOptions(c => c.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();


    // Cookie Authentication
    builder.Services
        // Using Cookie Authentication between Frontend and Backend
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        // We are going to use Cookies for ...
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax; // We don't want to deal with CSRF Tokens

            options.Events.OnRedirectToLogin = (context) =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = (context) =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                return Task.CompletedTask;
            };
        });

    // Add Policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(Policies.RequireUserRole, policy => policy.RequireRole(Roles.User));
        options.AddPolicy(Policies.RequireAdminRole, policy => policy.RequireRole(Roles.Administrator));
    });

    // Add the Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.OnRejected = (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

            return ValueTask.CompletedTask;
        };

        options.AddPolicy(Policies.PerUserRatelimit, context =>
        {
            var username = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return RateLimitPartition.GetTokenBucketLimiter(username, key =>
            {
                return new()
                {
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    AutoReplenishment = true,
                    TokenLimit = 100,
                    TokensPerPeriod = 100,
                    QueueLimit = 100,
                };
            });
        });
    });

    var app = builder.Build();

    // Use a Controller for handling the ASP.NET Core lower-level errors.
    app.UseExceptionHandler("/error");
    app.UseStatusCodePagesWithReExecute("/error/{0}");

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // CORS
    app.UseCors("CorsPolicy");

    app.UseRateLimiter();

    app.UseHttpsRedirection();

    app.UseAuthorization();
    app.UseRateLimiter();
    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "An unhandeled exception occured.");
}
finally
{
    // Wait 0.5 seconds before closing and flushing, to gather the last few logs.
    await Task.Delay(TimeSpan.FromMilliseconds(500));
    await Log.CloseAndFlushAsync();
}

public partial class Program { }
// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Options;
using ElasticsearchFulltextExample.Web.Services;
using ElasticsearchFulltextExample.Web.Hosting;

namespace ElasticsearchFulltextExample.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add CORS:
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policyBuilder =>
                {
                    policyBuilder
                        .WithOrigins("http://localhost:4200", "http://localhost:9000")
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                    ;
                });
            });

            // Use the Options Module:
            services.AddOptions();

            // Configure all Options Here:
            ConfigureOptions(services);

            // Register Hosted Services:
            RegisterHostedServices(services);

            // Register Application Specific Services here ...
            RegisterApplicationServices(services);

            // Use a fixed Machine Key, so the Machine Key isn't regenerated for each restart:
            services.AddDataProtection()
                .SetApplicationName("sample-app")
                .PersistKeysToFileSystem(new DirectoryInfo(@"D:\data"));

            // Use Web Controllers:
            services.AddControllers();

            // We need this for Antiforgery to work:
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapFallbackToController("Index", "Home");
            });

        }

        private void ConfigureOptions(IServiceCollection services)
        {
            services.Configure<ApplicationOptions>(Configuration.GetSection("Application"));
            services.Configure<TesseractOptions>(Configuration.GetSection("Application:Tesseract"));
            services.Configure<ElasticsearchOptions>(Configuration.GetSection("Application:Elasticsearch"));
            services.Configure<IndexerOptions>(Configuration.GetSection("Application:Indexer"));
        }

        private void RegisterApplicationServices(IServiceCollection services)
        {
            services.AddSingleton<ElasticsearchClient>();
            services.AddSingleton<TesseractService>();
            services.AddSingleton<ElasticsearchIndexService>();
        }

        private void RegisterHostedServices(IServiceCollection services)
        {
            services.AddHostedService<DatabaseInitializerHostedService>();
            services.AddHostedService<ElasticsearchInitializerHostedService>();
            services.AddHostedService<DocumentIndexerHostedService>();
        }
    }
}
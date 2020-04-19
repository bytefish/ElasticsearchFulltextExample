// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using ElsevierFulltextApi;
using ElasticsearchFulltextExample.Web.Elasticsearch;
using System;

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

        private void RegisterApplicationServices(IServiceCollection services)
        {
            services.AddSingleton(GetElsevierApiClient());
            services.AddSingleton(GetElasticsearchClient());
        }

        private IElsevierFulltextApiClient GetElsevierApiClient()
        {
            // Read the Elsevier API Key from a File, so it isn't hardcoded here:
            var apiKey = File.ReadAllText(@"D:\elsevier_api_key.txt");

            // ... then create the API client using the API Key:
            return new ElsevierFulltextApiClient(apiKey);
        }

        private IElasticsearchClient GetElasticsearchClient()
        {
            var client = new ElasticsearchClient(new Uri("http://localhost:9200"), "articles");

            client.CreateIndex();

            return client;
        }
    }
}
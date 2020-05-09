using ElasticsearchFulltextExample.Web.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Hosting
{
    public class ElasticsearchInitializerHostedService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public ElasticsearchInitializerHostedService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var client = scope.ServiceProvider.GetRequiredService<ElasticsearchClient>();

                // Prepare Elasticsearch Database:
                var response = await client.ExistsAsync();

                if (!response.Exists)
                {
                    await client.CreateIndexAsync();
                    await client.CreatePipelineAsync();
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

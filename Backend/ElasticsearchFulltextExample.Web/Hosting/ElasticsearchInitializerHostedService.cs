// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Elasticsearch;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Hosting
{
    public class ElasticsearchInitializerHostedService : IHostedService
    {
        private readonly ElasticsearchClient elasticsearchClient;

        public ElasticsearchInitializerHostedService(ElasticsearchClient elasticsearchClient)
        {
            this.elasticsearchClient = elasticsearchClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Prepare Elasticsearch Database:
            var response = await elasticsearchClient.ExistsAsync();

            if (!response.Exists)
            {
                await elasticsearchClient.CreateIndexAsync();
                await elasticsearchClient.CreatePipelineAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Hosting
{
    public class ElasticsearchInitializerHostedService : IHostedService
    {
        private readonly ElasticsearchClient elasticsearchClient;
        private readonly ILogger<ElasticsearchInitializerHostedService> logger;

        public ElasticsearchInitializerHostedService(ILogger<ElasticsearchInitializerHostedService> logger, ElasticsearchClient elasticsearchClient)
        {
            this.logger = logger;
            this.elasticsearchClient = elasticsearchClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Now we can wait for the Shards to boot up
            var healthTimeout = TimeSpan.FromSeconds(50);

            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"Waiting for at least 1 Node and at least 1 Active Shard, with a Timeout of {healthTimeout.TotalSeconds} seconds.");
            }

            await elasticsearchClient.WaitForClusterAsync(healthTimeout, cancellationToken);

            // Prepare Elasticsearch Database:
            var indexExistsResponse = await elasticsearchClient.ExistsAsync(cancellationToken);

            if (!indexExistsResponse.Exists)
            {
                await elasticsearchClient.CreateIndexAsync(cancellationToken);
                await elasticsearchClient.CreatePipelineAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

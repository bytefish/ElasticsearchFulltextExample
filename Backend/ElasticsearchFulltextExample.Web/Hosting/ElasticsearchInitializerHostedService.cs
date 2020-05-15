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
    public class ElasticsearchInitializerHostedService : BackgroundService
    {
        private readonly ElasticsearchClient elasticsearchClient;
        private readonly ILogger<ElasticsearchInitializerHostedService> logger;

        public ElasticsearchInitializerHostedService(ILogger<ElasticsearchInitializerHostedService> logger, ElasticsearchClient elasticsearchClient)
        {
            this.logger = logger;
            this.elasticsearchClient = elasticsearchClient;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var pingDelay = TimeSpan.FromSeconds(5);
            
            // We have to wait until the Elasticsearch Cluster is up and ready:
            while (!await IsServerReachableAsync(cancellationToken))
            {
                if (logger.IsWarningEnabled())
                {
                    logger.LogWarning($"Elasticsearch is not reachable. Retrying in {pingDelay.Seconds} seconds ...");
                }

                await Task.Delay(pingDelay, cancellationToken);
            }

            // Now we can wait for the Shards to boot up
            var healthTimeout = TimeSpan.FromSeconds(50);

            if(logger.IsDebugEnabled())
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

        private async Task<bool> IsServerReachableAsync(CancellationToken cancellationToken)
        {
            try
            {
                var pingResponse = await elasticsearchClient.PingAsync(cancellationToken);
                
                return pingResponse.IsValid;
            } 
            catch(Exception e)
            {
                if (logger.IsErrorEnabled())
                {
                    logger.LogError(e, "Ping to Elasticsearch Server failed with an Exception");
                }

                return false;
            }
        }
    }
}

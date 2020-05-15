// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Elasticsearch;
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
                logger.LogWarning($"Elasticsearch is not reachable. Retrying in {pingDelay.Seconds} seconds ...");

                await Task.Delay(pingDelay, cancellationToken);
            }

            // Now we can wait for the Shards to boot up
            var healthTimeout = TimeSpan.FromSeconds(50);

            await WaitForClusterAsync(healthTimeout, cancellationToken);

            // Prepare Elasticsearch Database:
            var response = await elasticsearchClient.ExistsAsync(cancellationToken);

            if (!response.Exists)
            {
                await elasticsearchClient.CreateIndexAsync();
                var res = await elasticsearchClient.CreatePipelineAsync(cancellationToken);

                logger.LogWarning(res.DebugInformation);
            }
        }

        private async Task<bool> IsServerReachableAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = await elasticsearchClient.Client.PingAsync(ct: cancellationToken);
                
                return response.IsValid;
            } 
            catch(Exception e)
            {
                logger.LogError(e, "Ping to Elasticsearch Server failed with an Exception");

                return false;
            }
        }


        private async Task WaitForClusterAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
                await elasticsearchClient.Client.Cluster.HealthAsync(
                    selector: x => x.WaitForNodes(">=1").WaitForActiveShards(">=1").Timeout(timeout),
                    ct: cancellationToken);
        }
    }
}

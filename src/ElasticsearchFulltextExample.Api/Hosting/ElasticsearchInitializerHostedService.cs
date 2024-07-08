// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Elasticsearch;
using ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch;
using ElasticsearchFulltextExample.Api.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Api.Hosting
{
    public class ElasticsearchInitializerHostedService : IHostedService
    {
        private readonly ElasticsearchSearchClient _elasticsearchClient;
        private readonly ILogger<ElasticsearchInitializerHostedService> logger;

        public ElasticsearchInitializerHostedService(ILogger<ElasticsearchInitializerHostedService> logger, ElasticsearchSearchClient elasticsearchClient)
        {
            this.logger = logger;
            this._elasticsearchClient = elasticsearchClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var healthTimeout = TimeSpan.FromSeconds(50);

            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"Waiting for at least 1 Node, with a Timeout of '{healthTimeout.TotalSeconds}' seconds.");
            }

            await _elasticsearchClient.WaitForClusterAsync(healthTimeout, cancellationToken);

            var indexExistsResponse = await _elasticsearchClient.IndexExistsAsync(cancellationToken);

            if (!indexExistsResponse.Exists)
            {
                await _elasticsearchClient.CreateIndexAsync(cancellationToken);
                await _elasticsearchClient.CreatePipelineAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

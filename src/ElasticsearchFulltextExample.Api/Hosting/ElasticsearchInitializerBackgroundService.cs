// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch;
using ElasticsearchFulltextExample.Shared.Infrastructure;

namespace ElasticsearchFulltextExample.Api.Hosting
{
    public class ElasticsearchInitializerBackgroundService : BackgroundService
    {
        private readonly ILogger<ElasticsearchInitializerBackgroundService> _logger;

        private readonly ElasticsearchSearchClient _elasticsearchClient;

        public ElasticsearchInitializerBackgroundService(ILogger<ElasticsearchInitializerBackgroundService> logger, ElasticsearchSearchClient elasticsearchClient)
        {
            _logger = logger;
            _elasticsearchClient = elasticsearchClient;
        }
        
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var healthTimeout = TimeSpan.FromSeconds(50);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug($"Waiting for at least 1 Node, with a Timeout of '{healthTimeout.TotalSeconds}' seconds.");
            }

            await _elasticsearchClient.WaitForClusterAsync(healthTimeout, cancellationToken);

            var indexExistsResponse = await _elasticsearchClient.IndexExistsAsync(cancellationToken);

            if (!indexExistsResponse.Exists)
            {
                await _elasticsearchClient.CreateIndexAsync(cancellationToken);
                await _elasticsearchClient.CreatePipelineAsync(cancellationToken);
            }
        }
    }
}

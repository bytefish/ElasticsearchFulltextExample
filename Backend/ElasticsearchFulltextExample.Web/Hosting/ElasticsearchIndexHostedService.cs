// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Database.Context;
using ElasticsearchFulltextExample.Web.Database.Model;
using ElasticsearchFulltextExample.Web.Options;
using ElasticsearchFulltextExample.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Hosting
{
    public class ElasticsearchIndexHostedService : BackgroundService
    {
        private readonly JobSchedulerOptions options;

        private readonly ILogger<ElasticsearchIndexHostedService> logger;
        private readonly ElasticsearchIndexService elasticsearchIndexService;

        public ElasticsearchIndexHostedService(ILogger<ElasticsearchIndexHostedService> logger, IOptions<JobSchedulerOptions> options, ElasticsearchIndexService elasticsearchIndexService)
        {
            this.logger = logger;
            this.options = options.Value;
            this.elasticsearchIndexService = elasticsearchIndexService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug($"ElasticsearchIndexHostedService is starting.");

            cancellationToken.Register(() => logger.LogDebug($"ElasticsearchIndexHostedService background task is stopping."));

            while (!cancellationToken.IsCancellationRequested)
            {
                logger.LogDebug($"ElasticsearchIndexHostedService task doing background work.");

                await IndexDocumentsAsync(cancellationToken);

                await Task.Delay(options.IndexDelay, cancellationToken);
            }

            logger.LogDebug($"ElasticsearchIndexHostedService background task is stopping.");
        }

        private async Task IndexDocumentsAsync(CancellationToken cancellationToken)
        {
            using(var context = new ApplicationDbContext())
            {
                var documents = context.Documents
                    .Where(x => x.Status == StatusEnum.Scheduled)
                    .AsNoTracking()
                    .AsAsyncEnumerable();

                await foreach(Document document in documents.WithCancellation(cancellationToken))
                {
                    try
                    {
                        await elasticsearchIndexService.IndexDocumentAsync(document, cancellationToken);

                        await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE documents SET status = {StatusEnum.Indexed}");
                    } 
                    catch(Exception e)
                    {
                        logger.LogError(e, $"Indexing Document '{document.Id}' failed");

                        await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE documents SET status = {StatusEnum.Failed}");
                    }
                }
            }
        }
    }
}

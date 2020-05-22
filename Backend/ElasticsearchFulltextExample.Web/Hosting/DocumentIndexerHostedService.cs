// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Database.Factory;
using ElasticsearchFulltextExample.Web.Database.Model;
using ElasticsearchFulltextExample.Web.Logging;
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
    public class DocumentIndexerHostedService : BackgroundService
    {
        private readonly IndexerOptions options;

        private readonly ILogger<DocumentIndexerHostedService> logger;
        private readonly ApplicationDbContextFactory applicationDbContextFactory;
        private readonly ElasticsearchIndexService elasticsearchIndexService;

        public DocumentIndexerHostedService(ILogger<DocumentIndexerHostedService> logger, IOptions<IndexerOptions> options, ApplicationDbContextFactory applicationDbContextFactory, ElasticsearchIndexService elasticsearchIndexService)
        {
            this.logger = logger;
            this.options = options.Value;
            this.elasticsearchIndexService = elasticsearchIndexService;
            this.applicationDbContextFactory = applicationDbContextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var indexDelay = TimeSpan.FromSeconds(options.IndexDelay);

            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"DocumentIndexer is starting with Index Delay: {options.IndexDelay} seconds.");
            }

            cancellationToken.Register(() => logger.LogDebug($"DocumentIndexer background task is stopping."));

            while (!cancellationToken.IsCancellationRequested)
            {
                if (logger.IsDebugEnabled())
                {
                    logger.LogDebug($"DocumentIndexer is running indexing loop.");
                }

                try
                {
                    await IndexDocumentsAsync(cancellationToken);
                } 
                catch(Exception e)
                {
                    logger.LogError(e, "Indexing failed due to an Exception");
                }

                await Task.Delay(indexDelay, cancellationToken);
            }

            logger.LogDebug($"DocumentIndexer exited the Index Loop.");
        }

        private async Task IndexDocumentsAsync(CancellationToken cancellationToken)
        {
            await IndexScheduledDocuments(cancellationToken);
            await RemoveDeletedDocuments(cancellationToken);

            async Task RemoveDeletedDocuments(CancellationToken cancellationToken)
            {
                using (var context = applicationDbContextFactory.Create())
                {
                    using (var transaction = await context.Database.BeginTransactionAsync())
                    {
                        var documents = await context.Documents
                            .Where(x => x.Status == StatusEnum.ScheduledDelete)
                            .AsNoTracking()
                            .ToListAsync(cancellationToken);

                        foreach (Document document in documents)
                        {
                            if (logger.IsInformationEnabled())
                            {
                                logger.LogInformation($"Removing Document: {document.Id}");
                            }

                            try
                            {
                                var deleteDocumentResponse = await elasticsearchIndexService.DeleteDocumentAsync(document, cancellationToken);

                                if (deleteDocumentResponse.IsValid)
                                {
                                    await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE documents SET status = {StatusEnum.Deleted}, indexed_at = {null} where id = {document.Id}");
                                }
                                else
                                {
                                    await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE documents SET status = {StatusEnum.Failed} where id = {document.Id}");
                                }
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, $"Removing Document '{document.Id}' failed");

                                await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE documents SET status = {StatusEnum.Failed} where id = {document.Id}");
                            }

                            if (logger.IsInformationEnabled())
                            {
                                logger.LogInformation($"Finished Removing Document: {document.Id}");
                            }
                        }

                        await transaction.CommitAsync();
                    }
                }
            }

            async Task IndexScheduledDocuments(CancellationToken cancellationToken)
            {
                using (var context = applicationDbContextFactory.Create())
                {
                    using (var transaction = await context.Database.BeginTransactionAsync())
                    {
                        var documents = await context.Documents
                            .Where(x => x.Status == StatusEnum.ScheduledIndex)
                            .AsNoTracking()
                            .ToListAsync(cancellationToken);

                        foreach (Document document in documents)
                        {
                            if (logger.IsInformationEnabled())
                            {
                                logger.LogInformation($"Start indexing Document: {document.Id}");
                            }

                            try
                            {
                                var indexDocumentResponse = await elasticsearchIndexService.IndexDocumentAsync(document, cancellationToken);

                                if (indexDocumentResponse.IsValid)
                                {
                                    await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE documents SET status = {StatusEnum.Indexed}, indexed_at = {DateTime.UtcNow} where id = {document.Id}");
                                } 
                                else
                                {
                                    await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE documents SET status = {StatusEnum.Failed}, indexed_at = {null} where id = {document.Id}");
                                }
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, $"Indexing Document '{document.Id}' failed");

                                await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE documents SET status = {StatusEnum.Failed}, indexed_at = {null} where id = {document.Id}");
                            }

                            if (logger.IsInformationEnabled())
                            {
                                logger.LogInformation($"Finished indexing Document: {document.Id}");
                            }
                        }

                        await transaction.CommitAsync();
                    }
                }
            }
        }
    }
}
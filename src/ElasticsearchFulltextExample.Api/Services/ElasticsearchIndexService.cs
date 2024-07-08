// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using ElasticsearchFulltextExample.Api.Database.Model;
using ElasticsearchFulltextExample.Api.Elasticsearch;
using ElasticsearchFulltextExample.Api.Elasticsearch.Model;
using ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch.Models;
using ElasticsearchFulltextExample.Api.Infrastructure.Exceptions;
using ElasticsearchFulltextExample.Api.Infrastructure.Tesseract;
using ElasticsearchFulltextExample.Api.Logging;
using ElasticsearchFulltextExample.Database;
using ElasticsearchFulltextExample.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace ElasticsearchFulltextExample.Api.Services
{
    public class ElasticsearchIndexService
    {
        private readonly ILogger<ElasticsearchIndexService> logger;

        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        
        private readonly TesseractExecutor _tesseractExecutor;
        private readonly ElasticsearchClient elasticsearchClient;

        public ElasticsearchIndexService(ILogger<ElasticsearchIndexService> logger, ElasticsearchClient elasticsearchClient, TesseractExecutor tesseractExecutor)
        {
            this.logger = logger;
            this.elasticsearchClient = elasticsearchClient;
            _tesseractExecutor = tesseractExecutor;
        }

        public async Task IndexDocumentAsync(int documentId, CancellationToken cancellationToken)
        {
            using var context = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var document = await context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == documentId, cancellationToken);

            if(document == null)
            {
                throw new EntityNotFoundException
                {
                    EntityName = nameof(Document),
                    EntityId = documentId,
                };
            }

            // Get the OCR Data for this Document
            var ocr = await _tesseractExecutor
                .ProcessDocument(document.Data, TesseractConstants.LanguageEng, cancellationToken)
                .ConfigureAwait(false);

            // Load the Keywords for the Document
            var keywords = await GetKeywordsByDocumentId(context, documentId, cancellationToken)
                .ConfigureAwait(false);

            // Load the Suggestions for the Document
            var suggestions = await GetSuggestionsByDocumentId(context, documentId, cancellationToken)
                .ConfigureAwait(false);

            // Build the Document to send to Elasticsearch
            var elasticsearchDocument = new ElasticsearchDocument
            {
                Id = document.Id.ToString(),
                Title = document.Title,
                Filename = document.Filename,
                Suggestions = suggestions
                    .Select(x => x.Name)
                    .ToArray(),
                Keywords = keywords
                    .Select(x => x.Name)
                    .ToArray(),
                Data = document.Data,
                Ocr = ocr,
                IndexedOn = DateTime.UtcNow,
                Attachment = null // Will be populated by the Elasticsearch Ingestion Pipeline
            };

            return await elasticsearchClient.IndexAsync(elasticsearchDocument, cancellationToken);
        }

        private async Task<List<Suggestion>> GetSuggestionsByDocumentId(ApplicationDbContext context, int documentId, CancellationToken cancellationToken)
        {
            var suggestionQueryable = from document in context.Documents
                              join documentSuggestion in context.DocumentSuggestions
                                  on document.Id equals documentSuggestion.DocumentId
                              join suggestion in context.Suggestions
                                  on documentSuggestion.SuggestionId equals suggestion.Id
                              where
                                document.Id == documentId
                              select suggestion;

            List<Suggestion> suggestions = await suggestionQueryable.AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return suggestions;
        }

        private async Task<List<Keyword>> GetKeywordsByDocumentId(ApplicationDbContext context, int documentId, CancellationToken cancellationToken)
        {
            var keywordQueryable = from document in context.Documents
                                      join documentKeyword in context.DocumentKeywords
                                          on document.Id equals documentKeyword.DocumentId
                                      join keyword in context.Keywords
                                          on documentKeyword.KeywordId equals keyword.Id
                                      where
                                        document.Id == documentId
                                      select keyword;

            List<Keyword> keywords = await keywordQueryable.AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return keywords;
        }


        public async Task<DeleteResponse> DeleteDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            return await elasticsearchClient
                .DeleteAsync(document.Id.ToString(), cancellationToken);
        }

        public async Task<PingResponse> PingAsync(CancellationToken cancellationToken)
        {
            return await elasticsearchClient.PingAsync(cancellationToken: cancellationToken);
        }

        private async Task<string> GetOcrDataAsync(Document document)
        {
            if (!document.IsOcrRequested)
            {
                if (logger.IsDebugEnabled())
                {
                    logger.LogDebug($"OCR Processing not requested for Document ID '{document.Id}'");
                }

                return string.Empty;
            }

            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"Running OCR for Document ID '{document.Id}'");
            }

            return await tesseractService
                .ProcessDocument(document.Data, "eng")
                .ConfigureAwait(false);
        }
    }
}

// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Database.Model;
using ElasticsearchFulltextExample.Api.Elasticsearch;
using ElasticsearchFulltextExample.Api.Logging;
using ElasticsearchFulltextExample.Web.Elasticsearch.Model;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Api.Services
{
    public class ElasticsearchIndexService
    {
        private readonly ILogger<ElasticsearchIndexService> logger;


        private readonly TesseractService tesseractService;
        private readonly ElasticsearchClient elasticsearchClient;

        public ElasticsearchIndexService(ILogger<ElasticsearchIndexService> logger, ElasticsearchClient elasticsearchClient, TesseractService tesseractService)
        {
            this.logger = logger;
            this.elasticsearchClient = elasticsearchClient;
            this.tesseractService = tesseractService;
        }

        public async Task<IndexResponse> IndexDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            var suggestions = _

            var elasticsearchDocument = new ElasticsearchDocument
            {
                Id = document.Id.ToString(),
                Title = document.Title,
                Filename = document.Filename,
                Suggestions = document.Suggestions,
                Keywords = document.Suggestions,
                Data = document.Data,
                Ocr = await GetOcrDataAsync(document),
                IndexedOn = DateTime.UtcNow,
            };

            return await elasticsearchClient.IndexAsync(elasticsearchDocument, cancellationToken);
        }

        public async Task<DeleteResponse> DeleteDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            return await elasticsearchClient.DeleteAsync(document.Id.ToString(), cancellationToken);
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

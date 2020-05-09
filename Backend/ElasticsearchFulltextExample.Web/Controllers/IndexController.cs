// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Database.Context;
using ElasticsearchFulltextExample.Web.Database.Model;
using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Elasticsearch.Model;
using ElasticsearchFulltextExample.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TinyCsvParser.Tokenizer;
using ITokenizer = TinyCsvParser.Tokenizer.ITokenizer;

namespace ElasticsearchFulltextExample.Web.Controllers
{
    public class IndexController : Controller
    {
        private readonly ITokenizer suggestionsTokenizer;
        private readonly TesseractService tesseractService;
        private readonly ElasticsearchClient elasticsearchClient;

        public IndexController(ElasticsearchClient elasticsearchClient, TesseractService tesseractService)
        {
            this.elasticsearchClient = elasticsearchClient;
            this.tesseractService = tesseractService;
            this.suggestionsTokenizer = new QuotedStringTokenizer(',');
        }

        [HttpPut]
        [Route("/api/index")]
        public async Task<IActionResult> IndexDocument([FromForm] DocumentDto documentDto, CancellationToken cancellationToken)
        {
            await ScheduleIndexing(documentDto, cancellationToken);

            return Ok();
        }

        private async Task ScheduleIndexing(DocumentDto documentDto, CancellationToken cancellationToken)
        {
            using (var context = new ApplicationDbContext())
            {
                bool.TryParse(documentDto.IsOcrRequested, out var isOcrRequest);

                var document = new Document
                {
                    DocumentId = documentDto.Id,
                    Title = documentDto.Title,
                    Filename = documentDto.File.FileName,
                    Suggestions = GetSuggestions(documentDto.Suggestions),
                    Keywords = GetSuggestions(documentDto.Suggestions),
                    Data = await GetBytesAsync(documentDto.File),
                    IsOcrRequested = isOcrRequest,
                    Status = StatusEnum.Scheduled
                };

                context.Documents.Add(document);

                await context.SaveChangesAsync(cancellationToken);
            }
        }


        private string[] GetSuggestions(string suggestions)
        {
            if(suggestions == null)
            {
                return null;
            }

            return suggestionsTokenizer
                .Tokenize(suggestions)
                .Select(x => x.Trim())
                .ToArray();
        }

        private async Task<byte[]> GetBytesAsync(IFormFile formFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
        }
    }
}
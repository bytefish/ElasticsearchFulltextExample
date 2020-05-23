// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Database.Factory;
using ElasticsearchFulltextExample.Web.Database.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<IndexController> logger;
        private readonly ITokenizer suggestionsTokenizer;
        private readonly ApplicationDbContextFactory applicationDbContextFactory;

        public IndexController(ILogger<IndexController> logger, ApplicationDbContextFactory applicationDbContextFactory)
        {
            this.logger = logger;
            this.suggestionsTokenizer = new QuotedStringTokenizer(',');
            this.applicationDbContextFactory = applicationDbContextFactory;
        }

        [HttpPost]
        [Route("/api/index")]
        public async Task<IActionResult> IndexDocument([FromForm] DocumentUploadDto documentDto, CancellationToken cancellationToken)
        {
            try
            {
                await ScheduleIndexing(documentDto, cancellationToken);

                return Ok();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to schedule document for Indexing");

                return StatusCode(500);
            }
        }

        private async Task ScheduleIndexing(DocumentUploadDto documentDto, CancellationToken cancellationToken)
        {
            using (var context = applicationDbContextFactory.Create())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    bool.TryParse(documentDto.IsOcrRequested, out var isOcrRequest);

                    var document = new Document
                    {
                        Title = documentDto.Title,
                        Filename = documentDto.File.FileName,
                        Suggestions = GetSuggestions(documentDto.Suggestions),
                        Keywords = GetSuggestions(documentDto.Suggestions),
                        Data = await GetBytesAsync(documentDto.File),
                        IsOcrRequested = isOcrRequest,
                        UploadedAt = DateTime.UtcNow,
                        Status = StatusEnum.ScheduledIndex
                    };

                    context.Documents.Add(document);

                    await context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync();
                }
            }
        }

        private string[] GetSuggestions(string suggestions)
        {
            if (suggestions == null)
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
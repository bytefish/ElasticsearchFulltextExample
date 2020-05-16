// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Database.Context;
using ElasticsearchFulltextExample.Web.Database.Factory;
using ElasticsearchFulltextExample.Web.Database.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TinyCsvParser.Tokenizer;
using ITokenizer = TinyCsvParser.Tokenizer.ITokenizer;

namespace ElasticsearchFulltextExample.Web.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ILogger<DocumentsController> logger;
        private readonly ITokenizer suggestionsTokenizer;
        private readonly ApplicationDbContextFactory applicationDbContextFactory;

        public DocumentsController(ILogger<DocumentsController> logger, ApplicationDbContextFactory applicationDbContextFactory)
        {
            this.logger = logger;
            this.suggestionsTokenizer = new QuotedStringTokenizer(',');
            this.applicationDbContextFactory = applicationDbContextFactory;
        }

        [HttpPost]
        [Route("/api/document")]
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

        [HttpPatch]
        [Route("/api/document/{id}")]
        [Consumes("application/merge-patch+json")]
        public async Task<IActionResult> PatchDocument([FromRoute] string id, [FromBody] JsonDocument jsonDocument, CancellationToken cancellationToken)
        {
            try
            {
                using (var context = applicationDbContextFactory.Create())
                {
                    var document = await context.Documents.FirstOrDefaultAsync(x => x.DocumentId == id, cancellationToken);

                    if (document == null)
                    {
                        return NotFound();
                    }

                    PatchValues(jsonDocument, document);
                }

                return Ok();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to schedule document for Indexing");

                return StatusCode(500);
            }
        }

        // This is a very simple way to patch values, but I find it sufficient to diff between objects and 
        // it should be generic enough to adapt it to other projects. The idea is simple: We iterate the 
        // the JsonDocument and when we hit a path, we are setting the value based on the JsonElement 
        // value:
        private void PatchValues(JsonDocument jsonDocument, Document document)
        {
            PatchValue(jsonDocument, document, "documentId", (document, element) => document.DocumentId = element.GetString());
            PatchValue(jsonDocument, document, "data", (document, element) => document.Data = element.GetBytesFromBase64());
            PatchValue(jsonDocument, document, "filename", (document, element) => document.Filename = element.GetString());
            PatchValue(jsonDocument, document, "isOcrRequested", (document, element) => document.IsOcrRequested = element.GetBoolean());
            PatchValue(jsonDocument, document, "keywords", (document, element) => document.Keywords = element.EnumerateArray().Select(x => x.GetString()).ToArray());
            PatchValue(jsonDocument, document, "suggestions", (document, element) => document.Suggestions = element.EnumerateArray().Select(x => x.GetString()).ToArray());
            PatchValue(jsonDocument, document, "status", (document, element) => document.Status = (StatusEnum) Enum.Parse(typeof(StatusEnum), element.GetString()));
            PatchValue(jsonDocument, document, "title", (document, element) => document.Title = element.GetString());
        }

        private void PatchValue(JsonDocument jsonDocument, Document document, string path, Action<Document, JsonElement> patch)
        {
            if (jsonDocument.RootElement.TryGetProperty(path, out JsonElement element))
            {
                patch(document, element);
            }
        }

        private async Task ScheduleIndexing(DocumentUploadDto documentDto, CancellationToken cancellationToken)
        {
            using (var context = applicationDbContextFactory.Create())
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
                    UploadedAt = DateTime.UtcNow,
                    Status = StatusEnum.ScheduledIndex
                };

                context.Documents.Add(document);

                await context.SaveChangesAsync(cancellationToken);
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
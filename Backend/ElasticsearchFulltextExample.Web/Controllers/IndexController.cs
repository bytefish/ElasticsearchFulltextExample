// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Elasticsearch.Model;
using ElasticsearchFulltextExample.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.IO;
using System.Linq;
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
        public async Task<IActionResult> IndexDocument([FromForm] DocumentDto document)
        {
            var indexResponse = await elasticsearchClient.IndexAsync(new Document
            {
                Id = document.Id,
                Title = document.Title,
                Filename = document.File.FileName,
                IndexedOn = DateTime.UtcNow,
                Suggestions = GetSuggestions(document.Suggestions),
                Keywords = GetSuggestions(document.Suggestions),
                OCR = await GetOcrDataAsync(document),
                Data = await GetBase64Async(document.File)
            });

            if (indexResponse.IsValid)
            {
                return Ok();
            }

            return BadRequest();
        }


        private async Task<string> GetOcrDataAsync(DocumentDto document)
        {
            if(!document.OCR)
            {
                return string.Empty;
            }

            return await tesseractService.ProcessDocument(document, "eng")
                .ConfigureAwait(false);
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

        private async Task<string> GetBase64Async(IFormFile file)
        {
            if(file == null)
            {
                return null;
            }

            var bytes = await GetBytes(file);

            return Convert.ToBase64String(bytes);
        }

        private async Task<byte[]> GetBytes(IFormFile formFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
        }
    }
}
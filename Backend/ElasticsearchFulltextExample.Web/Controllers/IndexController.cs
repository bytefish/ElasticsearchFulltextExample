// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Elasticsearch.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TinyCsvParser.Tokenizer;

namespace ElasticsearchFulltextExample.Web.Controllers
{
    public class IndexController : Controller
    {
        private readonly ITokenizer suggestionsTokenizer;
        private readonly ElasticsearchClient elasticsearchClient;

        public IndexController(ElasticsearchClient elasticsearchClient)
        {
            this.elasticsearchClient = elasticsearchClient;
            this.suggestionsTokenizer = new QuotedStringTokenizer(',');
        }

        [HttpPut]
        [Route("/api/index")]
        public async Task<IActionResult> IndexDocument([FromForm] DocumentDto document)
        {
            var contentAsBase64 = await GetBase64Async(document.File);
            
            var indexResponse = await elasticsearchClient.IndexAsync(new Document
            {
                Id = document.Id,
                Title = document.Title,
                Filename = document.File.FileName,
                IndexedOn = DateTime.UtcNow,
                Suggestions = GetSuggestions(document.Suggestions),
                Keywords = GetSuggestions(document.Suggestions),
                Data = contentAsBase64
            });

            if(indexResponse.IsValid)
            {
                return Ok();
            }

            return BadRequest();
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
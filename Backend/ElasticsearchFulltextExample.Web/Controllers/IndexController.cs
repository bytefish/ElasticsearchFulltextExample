// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Elasticsearch.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Controllers
{
    public class IndexController : Controller
    {
        private readonly ElasticsearchClient elasticsearchClient;

        public IndexController(ElasticsearchClient elasticsearchClient)
        {
            this.elasticsearchClient = elasticsearchClient;
        }

        [HttpPut]
        [Route("/api/index")]
        public async Task<IActionResult> IndexDocument([FromForm] DocumentDto document)
        {
            var contentAsBase64 = await GetBase64Async(document.File);

            var indexResponse = await elasticsearchClient.IndexAsync(new Document
            {
                Id = document.Id,
                Content = contentAsBase64
            });

            if(indexResponse.IsValid)
            {
                return Ok();
            }

            return BadRequest();
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
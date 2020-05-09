// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Elasticsearch.Model;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Controllers
{
    public class FileController : Controller
    {
        private readonly ElasticsearchClient elasticsearchClient;

        public FileController(ElasticsearchClient elasticsearchClient)
        {
            this.elasticsearchClient = elasticsearchClient;
        }

        [HttpGet]
        [Route("/api/files/{id}")]
        public async Task<IActionResult> Query([FromRoute(Name = "id")] string id)
        {
            var response = await elasticsearchClient.GetDocumentByIdAsync(id);

            if(!response.Found)
            {
                return NotFound();
            }

            var document = response.Source;

            if(document.Attachment == null)
            {
                return NoContent();
            }

            return BuildResult(document);
        }

        private FileContentResult BuildResult(ElasticsearchDocument document)
        {
            if(document == null)
            {
                return null;
            }

            var fileName = document.Filename;
            var fileType = GetContentType(document.Attachment);
            var fileBytes = document.Data;

            return File(fileBytes, fileType, fileName);
        }


        private string GetContentType(Attachment attachment)
        {
            if(attachment == null)
            {
                return System.Net.Mime.MediaTypeNames.Application.Octet;
            }

            if(string.IsNullOrWhiteSpace(attachment.ContentType))
            {
                return System.Net.Mime.MediaTypeNames.Application.Octet;
            }

            return attachment.ContentType;
        }
    }
}
// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Database.Factory;
using ElasticsearchFulltextExample.Web.Database.Model;
using ElasticsearchFulltextExample.Web.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Controllers
{
    public class FileController : Controller
    {
        private readonly ILogger<FileController> logger;
        private readonly ApplicationDbContextFactory applicationDbContextFactory;
        private readonly FileExtensionContentTypeProvider fileExtensionContentTypeProvider;

        public FileController(ILogger<FileController> logger, ApplicationDbContextFactory applicationDbContextFactory)
        {
            this.logger = logger;
            this.applicationDbContextFactory = applicationDbContextFactory;
            this.fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
        }

        [HttpGet]
        [Route("/api/files/{id}")]
        public async Task<IActionResult> GetFileById([FromRoute(Name = "id")] int id)
        {
            if(logger.IsDebugEnabled())
            {
                logger.LogDebug($"Downloading File with Document ID '{id}'");
            }

            using(var context = applicationDbContextFactory.Create())
            {
                var document = await context.Documents
                    .FirstOrDefaultAsync(x => x.Id == id);

                if(document == null)
                {
                    if(logger.IsDebugEnabled())
                    {
                        logger.LogDebug($"No File with Document ID '{id}' found");
                    }

                    return NotFound();
                }

                return BuildResult(document);
            }
        }

        private FileContentResult BuildResult(Document document)
        {
            if(document == null)
            {
                return null;
            }

            var fileName = document.Filename;
            var fileType = GetContentType(document);
            var fileBytes = document.Data;

            return File(fileBytes, fileType, fileName);
        }

        private string GetContentType(Document document)
        {
            if(document == null)
            {
                return System.Net.Mime.MediaTypeNames.Application.Octet;
            }

            if (string.IsNullOrWhiteSpace(document.Filename))
            {
                return System.Net.Mime.MediaTypeNames.Application.Octet;
            }

            if (!fileExtensionContentTypeProvider.TryGetContentType(document.Filename, out var contentType))
            {
                return System.Net.Mime.MediaTypeNames.Application.Octet;
            }

            return contentType;
        }
    }
}
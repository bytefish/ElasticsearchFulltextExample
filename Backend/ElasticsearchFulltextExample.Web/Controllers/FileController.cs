// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Database.Factory;
using ElasticsearchFulltextExample.Web.Database.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Controllers
{
    public class FileController : Controller
    {
        private readonly ApplicationDbContextFactory applicationDbContextFactory;
        private readonly FileExtensionContentTypeProvider fileExtensionContentTypeProvider;

        public FileController(ApplicationDbContextFactory applicationDbContextFactory)
        {
            this.applicationDbContextFactory = applicationDbContextFactory;
            this.fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
        }

        [HttpGet]
        [Route("/api/files/{id}")]
        public async Task<IActionResult> Query([FromRoute(Name = "id")] string id)
        {
            using(var context = applicationDbContextFactory.Create())
            {
                var document = await context.Documents
                    .FirstOrDefaultAsync(x => x.DocumentId == id);

                if(document == null)
                {
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
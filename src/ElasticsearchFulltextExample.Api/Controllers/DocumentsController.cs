// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Logging;
using ElasticsearchFulltextExample.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchFulltextExample.Api.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ILogger<DocumentsController> _logger;
        
        private readonly DocumentService _documentService;

        public DocumentsController(ILogger<DocumentsController> logger, DocumentService documentService)
        {
            _logger = logger;
            _documentService = documentService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
            [FromForm(Name = "title")] string title, 
            [FromForm(Name = "suggestions")] List<string>? suggestions,
            [FromForm(Name = "keywords")] List<string>? keywords,
            [FromForm(Name = "data")] IFormFile data,
            CancellationToken cancellationToken)
        {
            var document = new Database.Model.Document
            {
                Title = title,
                Filename = data.FileName,
                Data = await GetBytesAsync(data),
                LastEditedBy = Constants.Users.DataConversionUserId
            };

            await _documentService
                .CreateDocumentAsync(document, suggestions, keywords, cancellationToken)
                .ConfigureAwait(false);

            return Ok();
        }

        private async Task<byte[]> GetBytesAsync(IFormFile formFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
        }


        [HttpGet("{documentId}")]
        public async Task<IActionResult> GetFileById([FromRoute(Name = "documentId")] int documentId, CancellationToken cancellationToken)
        {
            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug($"Downloading File with Document ID '{documentId}'");
            }

            var fileInformation = await _documentService
                .GetFileInformationByDocumentIdAsync(documentId, cancellationToken)
                .ConfigureAwait(false);

            return File(fileContents: fileInformation.Data, contentType: fileInformation.ContentType, fileDownloadName: fileInformation.Filename);
        }
    }
}
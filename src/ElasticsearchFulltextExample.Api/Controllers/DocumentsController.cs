// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Infrastructure.Errors;
using ElasticsearchFulltextExample.Api.Infrastructure.Exceptions;
using ElasticsearchFulltextExample.Api.Services;
using ElasticsearchFulltextExample.Shared.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchFulltextExample.Api.Controllers
{
    [Route("[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly ILogger<DocumentsController> _logger;

        private readonly DocumentService _documentService;
        private readonly ExceptionToApplicationErrorMapper _exceptionToApplicationErrorMapper;

        public DocumentsController(ILogger<DocumentsController> logger, DocumentService documentService, ExceptionToApplicationErrorMapper exceptionToApplicationErrorMapper)
        {
            _logger = logger;
            _documentService = documentService;
            _exceptionToApplicationErrorMapper = exceptionToApplicationErrorMapper;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
            [FromForm(Name = "title")] string title, 
            [FromForm(Name = "suggestions")] List<string>? suggestions,
            [FromForm(Name = "keywords")] List<string>? keywords,
            [FromForm(Name = "data")] IFormFile data,
            CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidModelStateException
                    {
                        ModelStateDictionary = ModelState
                    };
                }

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

                return Created();
            }
            catch (Exception exception)
            {
                return _exceptionToApplicationErrorMapper.CreateApplicationErrorResult(HttpContext, exception);
            }
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
            _logger.TraceMethodEntry();

            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidModelStateException
                    {
                        ModelStateDictionary = ModelState
                    };
                }

                if (_logger.IsDebugEnabled())
                {
                    _logger.LogDebug($"Downloading File with Document ID '{documentId}'");
                }

                var fileInformation = await _documentService
                    .GetFileInformationByDocumentIdAsync(documentId, cancellationToken)
                    .ConfigureAwait(false);

                return File(fileContents: fileInformation.Data, contentType: fileInformation.ContentType, fileDownloadName: fileInformation.Filename);
            }
            catch (Exception exception)
            {
                return _exceptionToApplicationErrorMapper.CreateApplicationErrorResult(HttpContext, exception);
            }
        }
    }
}
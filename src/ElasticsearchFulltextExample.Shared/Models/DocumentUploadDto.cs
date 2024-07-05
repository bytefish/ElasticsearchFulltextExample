// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchFulltextExample.Web.Contracts
{
    public class DocumentUploadDto
    {
        [FromForm(Name = "title")]
        public string Title { get; set; }

        [FromForm(Name = "suggestions")]
        public string Suggestions { get; set; }

        [FromForm(Name = "isOcrRequested")]
        public string IsOcrRequested { get; set; }

        [FromForm(Name = "file")]
        public IFormFile File { get; set; }
    }
}

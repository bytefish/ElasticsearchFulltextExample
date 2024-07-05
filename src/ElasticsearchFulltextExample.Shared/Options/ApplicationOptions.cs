// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Web.Options
{
    public class ApplicationOptions
    {
        public string BaseUri { get; set; }

        public TesseractOptions Tesseract { get; set; }
    }
}

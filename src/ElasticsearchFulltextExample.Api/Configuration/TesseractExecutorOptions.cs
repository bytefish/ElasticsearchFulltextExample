// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Api.Configuration
{
    public class TesseractExecutorOptions
    {
        /// <summary>
        /// Gets or sets the Tesseract Executable.
        /// </summary>
        public required string Executable { get; set; }

        /// <summary>
        /// Gets or sets the Tesseract Temporary Directory.
        /// </summary>
        public required string TempDirectory { get; set; }
    }
}

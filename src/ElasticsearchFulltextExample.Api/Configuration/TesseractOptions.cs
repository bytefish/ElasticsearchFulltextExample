// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Api.Configuration
{
    public class TesseractOptions
    {
        public required string Executable { get; set; }

        public required string TempDirectory { get; set; }
    }
}

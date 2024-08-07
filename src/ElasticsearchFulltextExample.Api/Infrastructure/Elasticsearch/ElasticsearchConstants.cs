﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch
{
    /// <summary>
    /// Constants used by the Frontend and Backend.
    /// </summary>
    public static class ElasticsearchConstants
    {
        /// <summary>
        /// A tag used to find the highlightning start position.
        /// </summary>
        public static readonly string HighlightStartTag = "elasticsearch→";

        /// <summary>
        /// A tag used to find the highlightning end position.
        /// </summary>
        public static readonly string HighlightEndTag = "←elasticsearch";
    }
}

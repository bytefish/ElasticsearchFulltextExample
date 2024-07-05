// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Nest;
using System;

namespace ElasticsearchFulltextExample.Web.Elasticsearch.Model
{
    public class ElasticsearchDocument
    {
        /// <summary>
        /// A unique document id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Title of the Document for Suggestion.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The Original Filename of the uploaded document.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The Data of the Document.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Keywords to filter for.
        /// </summary>
        public string[] Keywords { get; set; }

        /// <summary>
        /// Suggestions for the Autocomplete Field.
        /// </summary>
        public string[] Suggestions { get; set; }

        /// <summary>
        /// The Date the document was indexed on.
        /// </summary>
        public DateTime IndexedOn { get; set; }

        /// <summary>
        /// OCR Data.
        /// </summary>
        public string Ocr { get; set; }

        /// <summary>
        /// The Attachment.
        /// </summary>
        public Attachment Attachment { get; set; }
    }
}

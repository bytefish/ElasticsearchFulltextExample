// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace ElasticsearchFulltextExample.Web.Database.Model
{
    public class Document
    {
        /// <summary>
        /// A unique document id.
        /// </summary>
        public int Id { get; set; }

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
        /// OCR Data.
        /// </summary>
        public bool IsOcrRequested { get; set; }

        /// <summary>
        /// The Date the Document was uploaded.
        /// </summary>
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// The Date the Document was indexed at.
        /// </summary>
        public DateTime? IndexedAt { get; set; }

        /// <summary>
        /// The Document Status.
        /// </summary>
        public StatusEnum Status { get; set; }
    }
}

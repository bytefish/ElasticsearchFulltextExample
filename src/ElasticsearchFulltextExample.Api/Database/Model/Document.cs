// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace ElasticsearchFulltextExample.Api.Database.Model
{
    public class Document
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the original filename.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the OCR flag.
        /// </summary>
        public bool OCR { get; set; }

        /// <summary>
        /// Gets or sets the upload date.
        /// </summary>
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// Gets or sets the index date.
        /// </summary>
        public DateTime? IndexedAt { get; set; }

        /// <summary>
        /// Gets or sets the document status.
        /// </summary>
        public ProcessingStatusEnum Status { get; set; }
    }
}

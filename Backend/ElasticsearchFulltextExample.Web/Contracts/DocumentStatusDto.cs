// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
{
    public class DocumentStatusDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("isOcrRequested")]
        public bool IsOcrRequested { get; set; }

        [JsonPropertyName("status")]
        public StatusEnumDto Status { get; set; }
    }
}

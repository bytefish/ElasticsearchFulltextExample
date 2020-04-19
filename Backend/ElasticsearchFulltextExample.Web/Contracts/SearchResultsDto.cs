// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
{
    public class SearchResultsDto
    {
        [JsonPropertyName("query")]
        public string Query { get; set; }

        [JsonPropertyName("results")]
        public SearchResultDto[] Results { get; set; }

    }
}

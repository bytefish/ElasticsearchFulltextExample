// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Api.Models
{
    public class SearchResults
    {
        public required string Query { get; set; }

        public List<SearchResult> Results { get; set; } = new();
    }
}

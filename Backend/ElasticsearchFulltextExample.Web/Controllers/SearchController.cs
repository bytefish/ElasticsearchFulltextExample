// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Utils;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System.Linq;

namespace ElasticsearchFulltextExample.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly IElasticsearchClient elasticsearchClient;

        public SearchController(IElasticsearchClient elasticsearchClient)
        {
            this.elasticsearchClient = elasticsearchClient;
        }

        [HttpGet]
        [Route("/api/search")]
        public IActionResult Query([FromQuery(Name = "q")] string query)
        {
            var searchResponse = elasticsearchClient.Search(query);
            var searchResult = ConvertToSearchResults(query, searchResponse);

            return Ok(searchResult);
        }

        [HttpGet]
        [Route("/api/suggest")]
        public IActionResult Suggest([FromQuery(Name = "q")] string query)
        {
            var searchResponse = elasticsearchClient.Search(query);
            var searchSuggestions = ConvertToSearchSuggestions(query, searchResponse);
           
            return Ok(searchSuggestions);
        }

        private SearchSuggestionDto ConvertToSearchSuggestions(string query, ISearchResponse<Elasticsearch.Model.Document> searchResponse)
        {
            var suggestions = searchResponse
                // Get the Hits:
                .Hits
                // Convert the Hit into a SearchResultDto:
                .Select(x => x.Source.Title)
                // And convert to array:
                .ToArray();

            return new SearchSuggestionDto
            {
                Query = query,
                Results = suggestions
            };
        }

        private SearchResultsDto ConvertToSearchResults(string query, ISearchResponse<Elasticsearch.Model.Document> searchResponse)
        {
            var searchResults = searchResponse
                // Get the Hits:
                .Hits
                // Convert the Hit into a SearchResultDto:
                .Select(x => new SearchResultDto
                {
                    Identifier = x.Source.Id,
                    Title = x.Source.Title,
                    Text = GetSummaryText(x),
                    Type = "Article",
                    Url = $"http://fake.local/{x.Source.Id}"
                })
                // And convert to array:
                .ToArray();

            return new SearchResultsDto
            {
                Query = query,
                Results = searchResults
            };
        }

        private string GetSummaryText(IHit<Elasticsearch.Model.Document> hit)
        {
            var words = StringUtils.GetWords(hit.Source.Content, 50);

            return $"{words} ...";
        }
        
    }
}
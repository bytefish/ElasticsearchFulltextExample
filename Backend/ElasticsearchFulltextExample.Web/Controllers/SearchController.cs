// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationOptions applicationOptions;
        private readonly ElasticsearchClient elasticsearchClient;

        public SearchController(IOptions<ApplicationOptions> applicationOptions, ElasticsearchClient elasticsearchClient)
        {
            this.applicationOptions = applicationOptions.Value;
            this.elasticsearchClient = elasticsearchClient;
        }

        [HttpGet]
        [Route("/api/search")]
        public async Task<IActionResult> Query([FromQuery(Name = "q")] string query)
        {
            var searchResponse = await elasticsearchClient.SearchAsync(query);
            var searchResult = ConvertToSearchResults(query, searchResponse);

            return Ok(searchResult);
        }

        [HttpGet]
        [Route("/api/suggest")]
        public async Task<IActionResult> Suggest([FromQuery(Name = "q")] string query)
        {
            var searchResponse = await elasticsearchClient.SearchAsync(query);
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
                    Text = GetSearchBoxText(x.Highlight),
                    Url = $"{applicationOptions.BaseUri}/api/files/{x.Source.Id}"
                })
                // And convert to array:
                .ToArray();

            return new SearchResultsDto
            {
                Query = query,
                Results = searchResults
            };
        }

        private string GetSearchBoxText(IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlight)
        {
            if(highlight == null)
            {
                return null;
            }

            if(highlight.TryGetValue("attachment.content", out var matches))
            {
                return matches.FirstOrDefault();
            }

            return null;
        }

        
    }
}
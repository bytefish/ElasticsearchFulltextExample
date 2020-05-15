// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Contracts;
using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public async Task<IActionResult> Query([FromQuery(Name = "q")] string query, CancellationToken cancellationToken)
        {
            var searchResponse = await elasticsearchClient.SearchAsync(query, cancellationToken);
            var searchResult = ConvertToSearchResults(query, searchResponse);

            return Ok(searchResult);
        }

        [HttpGet]
        [Route("/api/suggest")]
        public async Task<IActionResult> Suggest([FromQuery(Name = "q")] string query, CancellationToken cancellationToken)
        {
            var searchResponse = await elasticsearchClient.SuggestAsync(query, cancellationToken);
            var searchSuggestions = ConvertToSearchSuggestions(query, searchResponse);
           
            return Ok(searchSuggestions);
        }

        private SearchSuggestionsDto ConvertToSearchSuggestions(string query, ISearchResponse<Elasticsearch.Model.ElasticsearchDocument> searchResponse)
        {
            return new SearchSuggestionsDto
            {
                Query = query,
                Results = GetSuggestions(searchResponse)
            };
        }

        private SearchSuggestionDto[] GetSuggestions(ISearchResponse<Elasticsearch.Model.ElasticsearchDocument> searchResponse)
        {
            if (searchResponse == null)
            {
                return null;
            }

            var suggest = searchResponse.Suggest;

            if (suggest == null)
            {
                return null;
            }

            if(!suggest.ContainsKey("suggest"))
            {
                return null;
            }

            var suggestions = suggest["suggest"];

            if (suggestions == null)
            {
                return null;
            }

            // What we are doing here...? The Complete Field Type has no simple 
            // way to highlight the matched Prefix / Infix. We are instead replacing 
            // the matched completions:
            var result = new List<SearchSuggestionDto>();

            foreach (var suggestion in suggestions)
            {
                var offset = suggestion.Offset;
                var length = suggestion.Length;

                foreach (var option in suggestion.Options)
                {
                    var text = option.Text;
                    var prefix = option.Text.Substring(offset, Math.Min(length, text.Length)); 
                    var highlight = ReplaceAt(option.Text, offset, length, $"<strong>{prefix}</strong>");

                    result.Add(new SearchSuggestionDto { Text = text, Highlight = highlight });
                }
            }

            return result.ToArray();
        }

        public static string ReplaceAt(string str, int index, int length, string replace)
        {
            return str
                .Remove(index, Math.Min(length, str.Length - index))
                .Insert(index, replace);
        }

        private SearchResultsDto ConvertToSearchResults(string query, ISearchResponse<Elasticsearch.Model.ElasticsearchDocument> searchResponse)
        {
            var searchResults = searchResponse
                // Get the Hits:
                .Hits
                // Convert the Hit into a SearchResultDto:
                .Select(x => new SearchResultDto
                {
                    Identifier = x.Source.Id,
                    Title = x.Source.Title,
                    Keywords = x.Source.Keywords,
                    Matches = GetMatches(x.Highlight),
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

        private string[] GetMatches(IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlight)
        {
            var matchesForOcr = GetMatchesForField(highlight, "ocr"); 
            var matchesForContent = GetMatchesForField(highlight, "attachment.content");

            return Enumerable
                .Concat(matchesForOcr, matchesForContent)
                .ToArray();
        }

        private string[] GetMatchesForField(IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlight, string field)
        {
            if(highlight == null)
            {
                return new string[] { };
            }

            if(highlight.TryGetValue(field, out var matches))
            {
                return matches.ToArray();
            }

            return new string[] { };
        }
    }
}
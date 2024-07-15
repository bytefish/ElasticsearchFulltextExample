// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Configuration;
using ElasticsearchFulltextExample.Api.Infrastructure.Errors;
using ElasticsearchFulltextExample.Api.Infrastructure.Exceptions;
using ElasticsearchFulltextExample.Api.Models;
using ElasticsearchFulltextExample.Api.Services;
using ElasticsearchFulltextExample.Shared.Infrastructure;
using ElasticsearchFulltextExample.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ElasticsearchFulltextExample.Api.Controllers
{
    public class SearchController : ControllerBase
    {
        private readonly ILogger<SearchController> _logger;

        private readonly ApplicationOptions _applicationOptions;
        private readonly ElasticsearchService _elasticsearchService;
        private readonly ExceptionToApplicationErrorMapper _exceptionToApplicationErrorMapper;

        public SearchController(ILogger<SearchController> logger, IOptions<ApplicationOptions> applicationOptions, ElasticsearchService elasticsearchService, ExceptionToApplicationErrorMapper exceptionToApplicationErrorMapper)
        {
            _logger = logger;
            _applicationOptions = applicationOptions.Value;
            _elasticsearchService = elasticsearchService;
            _exceptionToApplicationErrorMapper = exceptionToApplicationErrorMapper;
        }

        [HttpGet]
        [Route("/search")]
        public async Task<IActionResult> Query([FromQuery(Name = "q")] string query, CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidModelStateException
                    {
                        ModelStateDictionary = ModelState
                    };
                }

                if(_logger.IsDebugEnabled())
                {
                    _logger.LogDebug("Executing SearchQuery '{SearchQuery}'", query);
                }

                var searchResults = await _elasticsearchService
                    .SearchAsync(query, cancellationToken)
                    .ConfigureAwait(false);

                var searchResultsDto = Convert(searchResults);

                return Ok(searchResultsDto);
            }
            catch (Exception exception)
            {
                return _exceptionToApplicationErrorMapper.CreateApplicationErrorResult(HttpContext, exception);
            }
        }

        private SearchResultsDto Convert(SearchResults source)
        {
            return new SearchResultsDto
            {
                Query = source.Query,
                Results = Convert(source.Results)
            };
        }

        private List<SearchResultDto> Convert(List<SearchResult> source)
        {
            return source
                .Select(searchResult => Convert(searchResult))
                .ToList();
        }

        private SearchResultDto Convert(SearchResult source)
        {
            return new SearchResultDto
            {
                Identifier = source.Identifier,
                Title = source.Title,
                Keywords = source.Keywords,
                Matches = source.Matches,
                Url = source.Url
            };
        }

        [HttpGet]
        [Route("/suggest")]
        public async Task<IActionResult> Suggest([FromQuery(Name = "q")] string query, CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidModelStateException
                    {
                        ModelStateDictionary = ModelState
                    };
                }

                if (_logger.IsDebugEnabled())
                {
                    _logger.LogDebug("Getting Suggestions '{SearchSuggestions}'", query);
                }

                var searchSuggestions = await _elasticsearchService
                    .SuggestAsync(query, cancellationToken)
                    .ConfigureAwait(false);

                var searchSuggestionsDto = Convert(searchSuggestions);

                return Ok(searchSuggestions);
            }
            catch (Exception exception)
            {
                return _exceptionToApplicationErrorMapper.CreateApplicationErrorResult(HttpContext, exception);
            }
        }

        private SearchSuggestionsDto Convert(SearchSuggestions source)
        {
            return new SearchSuggestionsDto
            {
                Query = source.Query,
                Results = Convert(source.Results)
            };
        }

        private List<SearchSuggestionDto> Convert(List<SearchSuggestion> source)
        {
            return source
                .Select(suggestion => Convert(suggestion))
                .ToList();
        }

        private SearchSuggestionDto Convert(SearchSuggestion source)
        {
            return new SearchSuggestionDto
            {
                Text = source.Text,
                Highlight = source.Highlight
            };
        }
    }
}
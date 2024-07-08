// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Configuration;
using ElasticsearchFulltextExample.Api.Elasticsearch;
using ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch.Models;
using ElasticsearchFulltextExample.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Api.Controllers
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

    }
}
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using ElasticsearchFulltextExample.Api.Configuration;
using ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch.Models;
using ElasticsearchFulltextExample.Api.Infrastructure.Exceptions;
using ElasticsearchFulltextExample.Api.Logging;
using ElasticsearchFulltextExample.Api.Models;
using ElasticsearchFulltextExample.Database;
using ElasticsearchFulltextExample.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using ElasticsearchClient = ElasticsearchFulltextExample.Api.Elasticsearch.ElasticsearchClient;

namespace ElasticsearchFulltextExample.Api.Services
{
    public class ElasticsearchService
    {
        private readonly ILogger<ElasticsearchService> _logger;

        private readonly ApplicationOptions _options;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ElasticsearchClient _elasticsearchClient;

        public ElasticsearchService(ILogger<ElasticsearchService> logger, IOptions<ApplicationOptions> options, IDbContextFactory<ApplicationDbContext> dbContextFactory, ElasticsearchClient elasticsearchClient)
        {
            _logger = logger;
            _options = options.Value;
            _dbContextFactory = dbContextFactory;
            _elasticsearchClient = elasticsearchClient;
        }

        public async Task IndexDocumentAsync(int documentId, CancellationToken cancellationToken)
        {
            using var context = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var document = await context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == documentId, cancellationToken);

            if(document == null)
            {
                throw new EntityNotFoundException
                {
                    EntityName = nameof(Document),
                    EntityId = documentId,
                };
            }

            // Load the Keywords for the Document
            var keywords = await GetKeywordsByDocumentId(context, documentId, cancellationToken)
                .ConfigureAwait(false);

            // Load the Suggestions for the Document
            var suggestions = await GetSuggestionsByDocumentId(context, documentId, cancellationToken)
                .ConfigureAwait(false);

            // Build the Document to send to Elasticsearch
            var elasticsearchDocument = new ElasticsearchDocument
            {
                Id = document.Id.ToString(),
                Title = document.Title,
                Filename = document.Filename,
                Suggestions = suggestions
                    .Select(x => x.Name)
                    .ToArray(),
                Keywords = keywords
                    .Select(x => x.Name)
                    .ToArray(),
                Data = document.Data,
                IndexedOn = null, // Will be set after indexing ...
                Attachment = null // Will be set after indexing ...
            };

            return await _elasticsearchClient.IndexAsync(elasticsearchDocument, cancellationToken);
        }

        /// <summary>
        /// Loads the Suggestions for a Document given a Document ID.
        /// </summary>
        /// <param name="context">DbContext to read from</param>
        /// <param name="documentId">Document ID</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>List of Suggestions associated with a Document</returns>
        private async Task<List<Suggestion>> GetSuggestionsByDocumentId(ApplicationDbContext context, int documentId, CancellationToken cancellationToken)
        {
            // Join Documents, DocumentSuggestions and Suggestions
            var suggestionQueryable = from document in context.Documents
                              join documentSuggestion in context.DocumentSuggestions
                                  on document.Id equals documentSuggestion.DocumentId
                              join suggestion in context.Suggestions
                                  on documentSuggestion.SuggestionId equals suggestion.Id
                              where
                                document.Id == documentId
                              select suggestion;

            List<Suggestion> suggestions = await suggestionQueryable.AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return suggestions;
        }

        /// <summary>
        /// Loads the Keywords associated with a given Document.
        /// </summary>
        /// <param name="context">DbContext to use</param>
        /// <param name="documentId">Document ID</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>List of Keywords associated with a given Document</returns>
        private async Task<List<Keyword>> GetKeywordsByDocumentId(ApplicationDbContext context, int documentId, CancellationToken cancellationToken)
        {
            // Join Documents, DocumentKeywords and Keywords
            var keywordQueryable = from document in context.Documents
                                   join documentKeyword in context.DocumentKeywords
                                       on document.Id equals documentKeyword.DocumentId
                                   join keyword in context.Keywords
                                       on documentKeyword.KeywordId equals keyword.Id
                                   where
                                     document.Id == documentId
                                   select keyword;

            List<Keyword> keywords = await keywordQueryable.AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return keywords;
        }

        /// <summary>
        /// Searches the Database for a Document.
        /// </summary>
        /// <param name="query">Query Text</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Search Results found for the given query</returns>
        public async Task<SearchResults> SearchAsync(string query, CancellationToken cancellationToken)
        {
            var searchResponse = await _elasticsearchClient
                .SearchAsync(query, cancellationToken)
                .ConfigureAwait(false);

            var searchResults = ConvertToSearchResults(query, searchResponse);

            return searchResults;
        }

        /// <summary>
        /// Searches the Database for Suggestions.
        /// </summary>
        /// <param name="query">Query Text</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Suggestions found for the given query</returns>
        public async Task<SearchSuggestions> SuggestAsync(string query, CancellationToken cancellationToken)
        {
            var suggestResponse = await _elasticsearchClient
                .SuggestAsync(query, cancellationToken)
                .ConfigureAwait(false);

            var searchSuggestions = ConvertToSearchSuggestions(query, suggestResponse);

            return searchSuggestions;
        }

        /// <summary>
        /// Deletes a Document by its Document ID.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>The Delete Reponse from Elasticsearch</returns>
        public async Task<DeleteResponse> DeleteDocumentAsync(int documentId, CancellationToken cancellationToken)
        {
            return await _elasticsearchClient
                .DeleteAsync(documentId.ToString(CultureInfo.InvariantCulture), cancellationToken);
        }

        /// <summary>
        /// Pings the Elasticsearch Cluster.
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Ping Response from Elasticsearch</returns>
        public async Task<PingResponse> PingAsync(CancellationToken cancellationToken)
        {
            return await _elasticsearchClient.PingAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Converts from a <see cref="SearchResponse{TDocument}"> to <see cref="SearchSuggestions"/>.
        /// </summary>
        /// <param name="query">Query Text</param>
        /// <param name="searchResponse">Raw Search Response</param>
        /// <returns>Converted Search Suggestions</returns>
        private SearchSuggestions ConvertToSearchSuggestions(string query, SearchResponse<ElasticsearchDocument> searchResponse)
        {
            return new SearchSuggestions
            {
                Query = query,
                Results = GetSuggestions(searchResponse)
            };
        }

        /// <summary>
        /// Gets the List of suggestions from a given <see cref="SearchResponse{TDocument}">.
        /// </summary>
        /// <param name="searchResponse">Raw Elasticsearch Search Response</param>
        /// <returns>Lust of Suggestions</returns>
        private List<SearchSuggestion> GetSuggestions(SearchResponse<ElasticsearchDocument> searchResponse)
        {
            if (searchResponse == null)
            {
                return [];
            }

            var suggest = searchResponse.Suggest;

            if (suggest == null)
            {
                return [];
            }

            if (!suggest.ContainsKey("suggest"))
            {
                return [];
            }

            var suggestions = suggest["suggest"];

            if (suggestions == null)
            {
                return [];
            }

            var result = new List<SearchSuggestion>();

            foreach (var suggestion in suggestions)
            {
                var completionSuggest = suggestion as CompletionSuggest<ElasticsearchDocument>;

                // This is not a Completion Suggest...
                if(completionSuggest == null)
                {
                    if(_logger.IsInformationEnabled())
                    {
                        _logger.LogInformation("Suggestion {Suggestion} is no Completion Suggestion. Ignored.", suggest);
                    }

                    continue;
                }

                var offset = completionSuggest.Offset;
                var length = completionSuggest.Length;

                foreach (var option in completionSuggest.Options)
                {
                    var text = option.Text;
                    var prefix = option.Text.Substring(offset, Math.Min(length, text.Length));
                    var highlight = ReplaceAt(option.Text, offset, length, $"<strong>{prefix}</strong>");

                    result.Add(new SearchSuggestion { Text = text, Highlight = highlight });
                }
            }

            return result;
        }

        /// <summary>
        /// Replaces a string at a given index.
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="index">Start Index</param>
        /// <param name="length">Length</param>
        /// <param name="replace">Replacement Value</param>
        /// <returns></returns>
        public static string ReplaceAt(string str, int index, int length, string replace)
        {
            return str
                .Remove(index, Math.Min(length, str.Length - index))
                .Insert(index, replace);
        }

        /// <summary>
        /// Converts a raw <see cref="SearchResponse{TDocument}"/> to <see cref="SearchResults"/>.
        /// </summary>
        /// <param name="query">Original Query</param>
        /// <param name="searchResponse">Search Response from Elasticsearch</param>
        /// <returns>Search Results for a given Query</returns>
        private SearchResults ConvertToSearchResults(string query, SearchResponse<ElasticsearchDocument> searchResponse)
        {
            var hits = searchResponse.Hits;

            if(hits.Count == 0)
            {
                return new SearchResults
                {
                    Query = query,
                    Results = []
                };
            }

            List<SearchResult> searchResults = [];

            foreach(var hit in hits)
            {
                if(hit.Source == null)
                {
                    if(_logger.IsWarningEnabled()) 
                    {
                        _logger.LogWarning("Got a hit, but it has no source (Query = '{Query}', Hit = '{Hit}')", query, hit);
                    }

                    continue;
                }

                var searchResult = new SearchResult
                {
                    Identifier = hit.Source.Id,
                    Title = hit.Source.Title,
                    Keywords = hit.Source.Keywords.ToList(),
                    Matches = GetMatches(hit.Highlight),
                    Url = $"{_options.BaseUri}/api/files/{hit.Source.Id}"
                };

                searchResults.Add(searchResult);
            }

            return new SearchResults
            {
                Query = query,
                Results = searchResults
            };
        }
        
        private List<string> GetMatches(IReadOnlyDictionary<string, IReadOnlyCollection<string>>? highlight)
        {
            if(highlight == null)
            {
                return [];
            }

            return GetMatchesForField(highlight, "attachment.content");
        }

        private List<string> GetMatchesForField(IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlight, string field)
        {
            if (highlight == null)
            {
                return [];
            }

            if (!highlight.TryGetValue(field, out var matches))
            {
                return [];
            }

            return matches.ToList();
        }
    }
}

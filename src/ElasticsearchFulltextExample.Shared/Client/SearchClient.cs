// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Shared.Infrastructure;
using ElasticsearchFulltextExample.Web.Contracts;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Json;

namespace ElasticsearchFulltextExample.Shared.Client
{
    public class SearchClient
    {
        private readonly ILogger<SearchClient> _logger;
        private readonly HttpClient _httpClient;

        public SearchClient(ILogger<SearchClient> logger, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SearchResultsDto?> SearchAsync(string query, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var escapedQuery = Uri.EscapeDataString(query);

            var response = await _httpClient
                .GetAsync($"search?q={escapedQuery}", cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }

            return await response.Content
                .ReadFromJsonAsync<SearchResultsDto>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<SearchSuggestionsDto?> SuggestAsync(string query, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var escapedQuery = Uri.EscapeDataString(query);

            var response = await _httpClient
                .GetAsync($"suggest?q={escapedQuery}", cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }

            return await response.Content
                .ReadFromJsonAsync<SearchSuggestionsDto>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task CreateSearchIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsync("create-index", null, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }
        }

        public async Task DeleteSearchIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsync("delete-index", null, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }
        }

        public async Task DeleteAllDocumentsAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsync("delete-all-documents", null, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }
        }

        public async Task IndexGitRepositoryAsync(GitRepositoryMetadataDto repositoryMetadata, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsJsonAsync("index-git-repository", repositoryMetadata, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }
        }

        public async Task IndexGitHubOrganizationAsync(IndexGitHubOrganizationRequestDto indexOrganizationRequest, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsJsonAsync("index-github-organization", indexOrganizationRequest, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }
        }

        public async Task IndexGitHubRepositoryAsync(IndexGitHubRepositoryRequestDto indexRepositoryRequest, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsJsonAsync("index-github-repository", indexRepositoryRequest, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }
        }

        public async Task IndexDocumentsAsync(List<CodeSearchDocumentDto> documents, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .PostAsJsonAsync("index-documents", documents, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }
        }

        public async Task<List<CodeSearchStatisticsDto>?> SearchStatisticsAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var response = await _httpClient
                .GetAsync("search-statistics", cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture,
                    "HTTP Request failed with Status: '{0}' ({1})",
                    (int)response.StatusCode,
                    response.StatusCode))
                {
                    StatusCode = response.StatusCode
                };
            }

            return await response.Content
                .ReadFromJsonAsync<List<CodeSearchStatisticsDto>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
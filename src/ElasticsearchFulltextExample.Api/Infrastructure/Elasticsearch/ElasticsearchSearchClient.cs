// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Transport;
using Microsoft.Extensions.Options;
using ElasticsearchCodeSearch.Shared.Elasticsearch;
using ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch.Models;
using Elastic.Clients.Elasticsearch.Ingest;
using ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch.Constants;
using ElasticsearchFulltextExample.Shared.Infrastructure;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch
{
    public class ElasticsearchSearchClient
    {
        private readonly ILogger<ElasticsearchSearchClient> _logger;

        private readonly ElasticsearchClient _client;
        private readonly string _indexName;

        public ElasticsearchSearchClient(ILogger<ElasticsearchSearchClient> logger, IOptions<ElasticsearchSearchClientOptions> options)
        {
            _logger = logger;
            _indexName = options.Value.IndexName;
            _client = CreateClient(options.Value);
        }

        private ElasticsearchClient CreateClient(ElasticsearchSearchClientOptions options)
        {
            _logger.TraceMethodEntry();

            var settings = new ElasticsearchClientSettings(new Uri(options.Uri))
                .CertificateFingerprint(options.CertificateFingerprint)
                .Authentication(new BasicAuthentication(options.Username, options.Password));

            return new ElasticsearchClient(settings);
        }

        public async Task<Elastic.Clients.Elasticsearch.IndexManagement.ExistsResponse> IndexExistsAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var indexExistsResponse = await _client.Indices
                .ExistsAsync(_indexName, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("ExistsResponse DebugInformation: {DebugInformation}", indexExistsResponse.DebugInformation);
            }

            return indexExistsResponse;
        }

        public async Task<PingResponse> PingAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var pingResponse = await _client
                .PingAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Ping DebugInformation: {DebugInformation}", pingResponse.DebugInformation);
            }

            return pingResponse;
        }

        public async Task<PutPipelineResponse> CreatePipelineAsync(CancellationToken cancellationToken)
        {
            var putPipelineResponse = await _client.Ingest.PutPipelineAsync("attachments", p => p
                .Description("Document attachment pipeline")
                .Processors(pr => pr
                    .Attachment<ElasticsearchDocument>(a => a
                        .Field(new Field("data"))
                        .TargetField(new Field("attachment"))
                    .RemoveBinary(true))), cancellationToken).ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug($"PutPipelineResponse DebugInformation: {putPipelineResponse.DebugInformation}");
            }

            return putPipelineResponse;
        }

        public async Task<CreateIndexResponse> CreateIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var createIndexResponse = await _client.Indices.CreateAsync(_indexName, descriptor => descriptor
                .Mappings(mapping => mapping.Properties<ElasticsearchDocument>(properties => properties
                    .Text(ElasticConstants.DocumentNames.Id)
                    .Text(ElasticConstants.DocumentNames.Title)
                    .Text(ElasticConstants.DocumentNames.Filename)
                    .Binary(ElasticConstants.DocumentNames.Data)
                    .Date(ElasticConstants.DocumentNames.IndexedOn)
                    .Keyword(ElasticConstants.DocumentNames.Keywords)
                    .Completion(ElasticConstants.DocumentNames.Suggestions)
                    .Object(ElasticConstants.DocumentNames.Attachment, attachment => attachment
                            .Properties(attachmentProperties => attachmentProperties
                                .Text(ElasticConstants.AttachmentNames.Content)
                                .Text(ElasticConstants.AttachmentNames.Title)
                                .Text(ElasticConstants.AttachmentNames.Author)
                                .Date(ElasticConstants.AttachmentNames.Date)
                                .Text(ElasticConstants.AttachmentNames.Keywords)
                                .Text(ElasticConstants.AttachmentNames.ContentType)
                                .LongNumber(ElasticConstants.AttachmentNames.ContentLength)
                                .Text(ElasticConstants.AttachmentNames.Language)
                                .Text(ElasticConstants.AttachmentNames.Modified)
                                .Text(ElasticConstants.AttachmentNames.Format)
                                .Text(ElasticConstants.AttachmentNames.Identifier)
                                .Text(ElasticConstants.AttachmentNames.Contributor)
                                .Text(ElasticConstants.AttachmentNames.Coverage)
                                .Text(ElasticConstants.AttachmentNames.Modifier)
                                .Text(ElasticConstants.AttachmentNames.CreatorTool)
                                .Text(ElasticConstants.AttachmentNames.Publisher)
                                .Text(ElasticConstants.AttachmentNames.Relation)
                                .Text(ElasticConstants.AttachmentNames.Rights)))))
            , cancellationToken);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("CreateIndexResponse DebugInformation: {DebugInformation}", createIndexResponse.DebugInformation);
            }

            return createIndexResponse;
        }

        public Task<SearchResponse<ElasticsearchDocument>> SearchAsync(string query, CancellationToken cancellationToken)
        {
            return _client.SearchAsync<ElasticsearchDocument>(document => document
                // Query this Index:
                .Index(_indexName)
                // Setup the Highlighters:
                .Highlight(highlight => highlight
                    .Fields(fields => fields
                        .Add(new Field($"{ElasticConstants.DocumentNames.Attachment}.{ElasticConstants.AttachmentNames.Content}"), hf => hf
                            .Fragmenter(HighlighterFragmenter.Span)
                            .PreTags([ ElasticsearchConstants.HighlightStartTag ])
                            .PostTags([ ElasticsearchConstants.HighlightEndTag ])
                            .NumberOfFragments(0)
                        )
                    )
                )
                // Now kick off the query:
                .Query(q => q.MultiMatch(mm => mm
                    .Query(query)
                    .Type(TextQueryType.BoolPrefix)
                    .Fields(new[]
                    {
                        ElasticConstants.DocumentNames.Title,
                        ElasticConstants.DocumentNames.Filename,
                        $"{ElasticConstants.DocumentNames.Attachment}.{ElasticConstants.AttachmentNames.Content}"
                    }))), cancellationToken);
        }

        public Task<SearchResponse<ElasticsearchDocument>> SuggestAsync(string query, CancellationToken cancellationToken)
        {
            return _client.SearchAsync<ElasticsearchDocument>(q => q
                .Index(_indexName)
                // Suggest Titles:
                .Suggest(suggest => suggest.Suggesters(suggesters => suggesters
                    .Add("suggest", s => s
                        .Text(query)
                        .Completion(c => c
                            .SkipDuplicates(true)
                            .Field(x => x.Suggestions))))), cancellationToken);
        }

        public async Task<IndicesStatsResponse> GetSearchStatisticsAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var indicesStatResponse = await _client
                .Indices.StatsAsync(request => request.Indices(_indexName))
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("IndicesStatsResponse DebugInformation: {DebugInformation}", indicesStatResponse.DebugInformation);
            }

            return indicesStatResponse;
        }

        public async Task<DeleteResponse> DeleteByIdAsync(int documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var deleteResponse = await _client
                .DeleteAsync<ElasticsearchDocument>(_indexName, documentId.ToString(), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("DeleteResponse DebugInformation: {DebugInformation}", deleteResponse.DebugInformation);
            }

            return deleteResponse;
        }

        public async Task<DeleteByQueryResponse> DeleteAllAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var deleteByQueryResponse = await _client
                .DeleteByQueryAsync<ElasticsearchDocument>(_indexName, request => request.Query(query => query.MatchAll(x => { })), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("DeleteResponse DebugInformation: {DebugInformation}", deleteByQueryResponse.DebugInformation);
            }

            return deleteByQueryResponse;
        }

        public async Task<DeleteIndexResponse> DeleteIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var deleteIndexResponse = await _client.Indices.DeleteAsync(_indexName).ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("DeleteIndexResponse DebugInformation: {DebugInformation}", deleteIndexResponse.DebugInformation);
            }

            return deleteIndexResponse;
        }

        public async Task<IndicesStatsResponse> GetSearchStatistics(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var indicesStatResponse = await _client
                .Indices.StatsAsync(request => request.Indices(_indexName))
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("IndicesStatsResponse DebugInformation: {DebugInformation}", indicesStatResponse.DebugInformation);
            }

            return indicesStatResponse;
        }

        public async Task<DeleteResponse> DeleteAsync(string documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var deleteResponse = await _client
                .DeleteAsync<ElasticsearchDocument>(documentId, x => x.Index(_indexName), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("DeleteResponse DebugInformation: {DebugInformation}", deleteResponse.DebugInformation);
            }

            return deleteResponse;
        }

        public async Task<GetResponse<ElasticsearchDocument>> GetDocumentByIdAsync(string documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var getResponse = await _client
                .GetAsync<ElasticsearchDocument>(documentId, x => x.Index(_indexName), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("GetResponse DebugInformation: {DebugInformation}", getResponse.DebugInformation);
            }

            return getResponse;
        }

        public async Task<HealthResponse> WaitForClusterAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var healthRequest = new HealthRequest()
            {
                Timeout = timeout
            };

            var clusterHealthResponse = await _client.Cluster
                .HealthAsync(healthRequest, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("ClusterHealthResponse DebugInformation: {DebugInformation}", clusterHealthResponse.DebugInformation);
            }

            return clusterHealthResponse;
        }

        public async Task<UpdateResponse<ElasticsearchDocument>> UpdateAsync(ElasticsearchDocument document, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var updateResponse = await _client
                .UpdateAsync<ElasticsearchDocument, ElasticsearchDocument>(document, cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("UpdateResponse DebugInformation: {DebugInformation}", updateResponse.DebugInformation);
            }

            return updateResponse;
        }

        public async Task<BulkResponse> BulkIndexAsync(IEnumerable<ElasticsearchDocument> documents, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var bulkResponse = await _client.BulkAsync(b => b
                .Index(_indexName)
                .IndexMany(documents), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("BulkResponse DebugInformation: {DebugInformation}", bulkResponse.DebugInformation);
            }

            return bulkResponse;
        }

        public async Task<IndexResponse> IndexAsync(ElasticsearchDocument document, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var indexResponse = await _client
                .IndexAsync<ElasticsearchDocument>(document: document, idx => idx
                    .Index(_indexName)
                    .OpType(OpType.Index), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("IndexResponse DebugInformation: {DebugInformation}", indexResponse.DebugInformation);
            }

            return indexResponse;
        }
    }
}
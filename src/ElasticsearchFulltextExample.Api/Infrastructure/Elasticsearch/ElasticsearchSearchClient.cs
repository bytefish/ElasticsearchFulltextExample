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
using ElasticsearchFulltextExample.Shared.Infrastructure;
using ElasticsearchFulltextExample.Shared.Constants;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.Mapping;

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
            var putPipelineResponse = await _client.Ingest.PutPipelineAsync(ElasticConstants.Pipelines.Attachments, p => p
                .Description("Document attachment pipeline")
                .Processors(pr => pr
                    .Attachment<ElasticsearchDocument>(a => a
                        .Field(new Field(ElasticConstants.DocumentNames.Data))
                        .TargetField(new Field(ElasticConstants.DocumentNames.Attachment))
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
                .Settings(settings => settings
                    .Codec("best_compression")
                    .Analysis(analysis => analysis
                         .Tokenizers(tokenizers => tokenizers
                            .EdgeNGram("fts_tokenizer", ngram => ngram
                                .MinGram(2)
                                .MaxGram(10)
                                .TokenChars([TokenChar.Letter, TokenChar.Digit])))
                        .TokenFilters(filters => filters
                            .WordDelimiterGraph("word_delimiter_graph_filter", filter => filter
                                .PreserveOriginal(true)))
                        .Analyzers(analyzers => analyzers
                            .Custom("fts_analyzer", custom => custom
                                .Tokenizer("fts_tokenizer")
                                .Filter([ "lowercase", "asciifolding"])))))
                .Mappings(mapping => mapping
                    .Properties<ElasticsearchDocument>(properties => properties
                        .Text(ElasticConstants.DocumentNames.Id)
                        .Text(ElasticConstants.DocumentNames.Title, c => c
                            .Analyzer("fts_analyzer")
                            .Store(true))
                        .Text(ElasticConstants.DocumentNames.Filename)
                        .Binary(ElasticConstants.DocumentNames.Data)
                        .Date(ElasticConstants.DocumentNames.IndexedOn)
                        .Keyword(ElasticConstants.DocumentNames.Keywords)
                        .Completion(ElasticConstants.DocumentNames.Suggestions)
                        .Object(ElasticConstants.DocumentNames.Attachment, attachment => attachment
                                .Properties(attachmentProperties => attachmentProperties
                                    .Text(ElasticConstants.AttachmentNames.Content, c => c
                                        .IndexOptions(IndexOptions.Positions)
                                        .Analyzer("fts_analyzer")
                                        .TermVector(TermVectorOption.WithPositionsOffsetsPayloads)
                                        .Store(true))
                                    .Text(ElasticConstants.AttachmentNames.Title, c => c
                                        .Analyzer("fts_analyzer")
                                        .Store(true))
                                    .Text(ElasticConstants.AttachmentNames.Author, c => c
                                        .Analyzer("fts_analyzer")
                                        .Store(true))
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

        public async Task<SearchResponse<ElasticsearchDocument>> SearchAsync(string query, int from, int size, CancellationToken cancellationToken)
        {
            var results = await _client.SearchAsync<ElasticsearchDocument>(document => document
                // Query this Index:
                .Index(_indexName)
                // Paginate
                //.From(from).Size(size)
                // Setup the Highlighters:
                .Highlight(highlight => highlight
                    .Fields(fields => fields
                        .Add(new Field($"{ElasticConstants.DocumentNames.Attachment}.{ElasticConstants.AttachmentNames.Content}"), hf => hf
                            .Fragmenter(HighlighterFragmenter.Span)
                            .PreTags([ ElasticsearchConstants.HighlightStartTag ])
                            .PostTags([ ElasticsearchConstants.HighlightEndTag ])
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
                    }))), cancellationToken)
                .ConfigureAwait(false);

            return results;
        }

        public Task<SearchResponse<ElasticsearchDocument>> SuggestAsync(string query, CancellationToken cancellationToken)
        {
            return _client.SearchAsync<ElasticsearchDocument>(q => q
                // Index to Search:
                .Index(_indexName)
                // Suggest Titles:
                .Suggest(suggest => suggest.Suggesters(suggesters => suggesters
                    .Add("suggest", s => s
                        .Text(query)
                        .Completion(c => c
                            .SkipDuplicates(true)
                            .Field(new Field(ElasticConstants.DocumentNames.Suggestions)))))), cancellationToken);
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

        public async Task<DeletePipelineResponse> DeletePipelineAsync(string pipeline, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var deletePipelineResponse = await _client.Ingest
                .DeletePipelineAsync(pipeline)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("DeletePipelineResponse DebugInformation: {DebugInformation}", deletePipelineResponse.DebugInformation);
            }

            return deletePipelineResponse;
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
                .Pipeline(ElasticConstants.Pipelines.Attachments)
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
                    .Pipeline(ElasticConstants.Pipelines.Attachments)
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
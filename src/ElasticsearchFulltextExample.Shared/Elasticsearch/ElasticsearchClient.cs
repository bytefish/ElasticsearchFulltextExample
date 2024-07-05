// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elasticsearch.Net;
using ElasticsearchFulltextExample.Web.Elasticsearch.Model;
using ElasticsearchFulltextExample.Web.Logging;
using ElasticsearchFulltextExample.Web.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Elasticsearch
{
    public class ElasticsearchClient
    {
        private readonly ILogger<ElasticsearchClient> logger;
        private readonly IElasticClient client;
        private readonly string indexName;

        public ElasticsearchClient(ILogger<ElasticsearchClient> logger, IOptions<ElasticsearchOptions> options)
            : this(logger, CreateClient(options.Value.Uri), options.Value.IndexName)
        {

        }

        public ElasticsearchClient(ILogger<ElasticsearchClient> logger, IElasticClient client, string indexName)
        {
            this.logger = logger;
            this.indexName = indexName;
            this.client = client;
        }

        public async Task<ExistsResponse> ExistsAsync(CancellationToken cancellationToken)
        {
            var indexExistsResponse = await client.Indices.ExistsAsync(indexName, ct: cancellationToken);

            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"ExistsResponse DebugInformation: {indexExistsResponse.DebugInformation}");
            }

            return indexExistsResponse;
        }

        public async Task<PingResponse> PingAsync(CancellationToken cancellationToken)
        {
            var pingResponse = await client.PingAsync(ct: cancellationToken);

            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"Ping DebugInformation: {pingResponse.DebugInformation}");
            }

            return pingResponse;
        }

        public async Task<CreateIndexResponse> CreateIndexAsync(CancellationToken cancellationToken)
        {
            var createIndexResponse = await client.Indices.CreateAsync(indexName, descriptor =>
            {
                return descriptor.Map<ElasticsearchDocument>(mapping => mapping
                    .Properties(properties => properties
                        .Text(textField => textField.Name(document => document.Id))
                        .Text(textField => textField.Name(document => document.Title))
                        .Text(textField => textField.Name(document => document.Filename))
                        .Text(textField => textField.Name(document => document.Ocr))
                        .Binary(textField => textField.Name(document => document.Data))
                        .Date(dateField => dateField.Name(document => document.IndexedOn))
                        .Keyword(keywordField => keywordField.Name(document => document.Keywords))
                        .Completion(completionField => completionField.Name(document => document.Suggestions))
                        .Object<Attachment>(attachment => attachment
                            .Name(document => document.Attachment)
                            .Properties(attachmentProperties => attachmentProperties
                                .Text(t => t.Name(n => n.Name))
                                .Text(t => t.Name(n => n.Content))
                                .Text(t => t.Name(n => n.ContentType))
                                .Number(n => n.Name(nn => nn.ContentLength))
                                .Date(d => d.Name(n => n.Date))
                                .Text(t => t.Name(n => n.Author))
                                .Text(t => t.Name(n => n.Title))
                                .Text(t => t.Name(n => n.Keywords))))));
            }, cancellationToken);

            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"CreateIndexResponse DebugInformation: {createIndexResponse.DebugInformation}");
            }

            return createIndexResponse;
        }

        public async Task<DeleteResponse> DeleteAsync(string documentId, CancellationToken cancellationToken)
        {
            var deleteResponse = await client.DeleteAsync<ElasticsearchDocument>(documentId, x => x.Index(indexName), cancellationToken);

            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"DeleteResponse DebugInformation: {deleteResponse.DebugInformation}");
            }

            return deleteResponse;
        }

        public async Task<GetResponse<ElasticsearchDocument>> GetDocumentByIdAsync(string documentId, CancellationToken cancellationToken)
        {
            var getResponse = await client.GetAsync<ElasticsearchDocument>(documentId, x => x.Index(indexName), cancellationToken);
            
            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"GetResponse DebugInformation: {getResponse.DebugInformation}");
            }

            return getResponse;
        }

        public async Task<PutPipelineResponse> CreatePipelineAsync(CancellationToken cancellationToken)
        {
            var putPipelineResponse = await client.Ingest.PutPipelineAsync("attachments", p => p
                .Description("Document attachment pipeline")
                .Processors(pr => pr
                    .Attachment<ElasticsearchDocument>(a => a
                        .Field(f => f.Data)
                        .TargetField(f => f.Attachment))
                    .Remove<ElasticsearchDocument>(x => x.Field("data"))), cancellationToken);

            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"PutPipelineResponse DebugInformation: {putPipelineResponse.DebugInformation}");
            }

            return putPipelineResponse;
        }

        public async Task<ClusterHealthResponse> WaitForClusterAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var clusterHealthResponse = await client.Cluster.HealthAsync(selector: cluster => cluster
                .WaitForNodes("1")
                .WaitForActiveShards("1").Timeout(timeout), ct: cancellationToken);

            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"ClusterHealthResponse DebugInformation: {clusterHealthResponse.DebugInformation}");
            }

            return clusterHealthResponse;
        }

        public async Task<BulkResponse> BulkIndexAsync(IEnumerable<ElasticsearchDocument> documents, CancellationToken cancellationToken)
        {
            var request = new BulkDescriptor();

            foreach (var document in documents)
            {
                request.Index<ElasticsearchDocument>(op => op
                    .Id(document.Id)
                    .Index(indexName)
                    .Document(document)
                    .Pipeline("attachments"));
            }

            var bulkResponse = await client.BulkAsync(request, cancellationToken);

            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"BulkResponse DebugInformation: {bulkResponse.DebugInformation}");
            }

            return bulkResponse;
        }

        public async Task<IndexResponse> IndexAsync(ElasticsearchDocument document, CancellationToken cancellationToken)
        {
            var indexResponse = await client.IndexAsync(document, x => x
                .Id(document.Id)
                .Index(indexName)
                .Pipeline("attachments"), cancellationToken);

            if (logger.IsDebugEnabled())
            {
                logger.LogDebug($"IndexResponse DebugInformation: {indexResponse.DebugInformation}");
            }

            return indexResponse;
        }

        public Task<ISearchResponse<ElasticsearchDocument>> SearchAsync(string query, CancellationToken cancellationToken)
        {
            return client.SearchAsync<ElasticsearchDocument>(document => document
                // Query this Index:
                .Index(indexName)
                // Highlight Text Content:
                .Highlight(highlight => highlight
                    .Fields(
                        fields => fields
                            .Fragmenter(HighlighterFragmenter.Span)
                            .PreTags("<strong>")
                            .PostTags("</strong>")
                            .FragmentSize(150)
                            .NoMatchSize(150)
                            .NumberOfFragments(5)
                            .Field(x => x.Ocr),
                        fields => fields
                            .Fragmenter(HighlighterFragmenter.Span)
                            .PreTags("<strong>")
                            .PostTags("</strong>")
                            .FragmentSize(150)
                            .NoMatchSize(150)
                            .NumberOfFragments(5)
                            .Field(x => x.Attachment.Content))
                    )
                // Now kick off the query:
                .Query(q => q.MultiMatch(mm => mm
                    .Query(query)
                    .Type(TextQueryType.BoolPrefix)
                    .Fields(f => f
                        .Field(d => d.Keywords)
                        .Field(d => d.Ocr)
                        .Field(d => d.Attachment.Content)))), cancellationToken);
        }

        public Task<ISearchResponse<ElasticsearchDocument>> SuggestAsync(string query, CancellationToken cancellationToken)
        {
            return client.SearchAsync<ElasticsearchDocument>(x => x
                // Query this Index:
                .Index(indexName)
                // Suggest Titles:
                .Suggest(s => s
                    .Completion("suggest", x => x
                        .Prefix(query)
                        .SkipDuplicates(true)
                        .Field(x => x.Suggestions))), cancellationToken);
        }


        private static IElasticClient CreateClient(string uriString)
        {
            var connectionUri = new Uri(uriString);
            var connectionPool = new SingleNodeConnectionPool(connectionUri);
            var connectionSettings = new ConnectionSettings(connectionPool);

            return new ElasticClient(connectionSettings);
        }
    }
}
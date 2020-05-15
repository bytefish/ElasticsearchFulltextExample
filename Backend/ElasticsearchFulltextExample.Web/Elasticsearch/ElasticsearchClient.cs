// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elasticsearch.Net;
using ElasticsearchFulltextExample.Web.Database.Model;
using ElasticsearchFulltextExample.Web.Elasticsearch.Model;
using ElasticsearchFulltextExample.Web.Options;
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
        public readonly IElasticClient Client;
        public readonly string IndexName;

        public ElasticsearchClient(IOptions<ElasticsearchOptions> options)
            : this(CreateClient(options.Value.Uri), options.Value.IndexName)
        {
        }

        public ElasticsearchClient(IElasticClient client, string indexName)
        {
            IndexName = indexName;
            Client = client;
        }

        public Task<ExistsResponse> ExistsAsync(CancellationToken cancellationToken)
        {
            return Client.Indices.ExistsAsync(IndexName, ct: cancellationToken);
        }

        public Task<CreateIndexResponse> CreateIndexAsync()
        {
            return Client.Indices.CreateAsync(IndexName, descriptor =>
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
            });
        }

        public Task<GetResponse<ElasticsearchDocument>> GetDocumentByIdAsync(string documentId, CancellationToken cancellationToken)
        {
            return Client.GetAsync<ElasticsearchDocument>(documentId, x => x.Index(IndexName), cancellationToken);
        }

        public Task<PutPipelineResponse> CreatePipelineAsync(CancellationToken cancellationToken)
        {
            return Client.Ingest.PutPipelineAsync("attachments", p => p
                .Description("Document attachment pipeline")
                .Processors(pr => pr
                    .Attachment<ElasticsearchDocument>(a => a
                        .Field(f => f.Data)
                        .TargetField(f => f.Attachment))), cancellationToken);
        }

        public Task<BulkResponse> BulkIndexAsync(IEnumerable<ElasticsearchDocument> documents, CancellationToken cancellationToken)
        {
            var request = new BulkDescriptor();

            foreach (var document in documents)
            {
                request.Index<ElasticsearchDocument>(op => op
                    .Id(document.Id)
                    .Index(IndexName)
                    .Document(document)
                    .Pipeline("attachments"));
            }

            return Client.BulkAsync(request, cancellationToken);
        }

        public Task<IndexResponse> IndexAsync(ElasticsearchDocument document, CancellationToken cancellationToken)
        {
            return Client.IndexAsync(document, x => x
                .Id(document.Id)
                .Index(IndexName)
                .Pipeline("attachments"), cancellationToken);
        }

        public Task<ISearchResponse<ElasticsearchDocument>> SearchAsync(string query, CancellationToken cancellationToken)
        {
            return Client.SearchAsync<ElasticsearchDocument>(document => document
                // Query this Index:
                .Index(IndexName)
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
                .Query(q => BuildQueryContainer(query)), cancellationToken);
        }

        public Task<ISearchResponse<ElasticsearchDocument>> SuggestAsync(string query, CancellationToken cancellationToken)
        {
            return Client.SearchAsync<ElasticsearchDocument>(x => x
                // Query this Index:
                .Index(IndexName)
                // Suggest Titles:
                .Suggest(s => s
                    .Completion("suggest", x => x
                        .Prefix(query)
                        .SkipDuplicates(true)
                        .Field(x => x.Suggestions))), cancellationToken);
        }

        private QueryContainer BuildQueryContainer(string query)
        {
            return Query<ElasticsearchDocument>.MultiMatch(x => x.Query(query)
                .Type(TextQueryType.BoolPrefix)
                .Fields(f => f
                    .Field(x => x.Keywords)
                    .Field(x => x.Ocr)
                    .Field(x => x.Attachment.Content)));
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
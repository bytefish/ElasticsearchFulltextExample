using Elasticsearch.Net;
using ElasticsearchFulltextExample.Web.Elasticsearch.Model;
using Nest;
using System;
using System.Collections.Generic;

namespace ElasticsearchFulltextExample.Web.Elasticsearch
{
    public interface IElasticsearchClient
    {
        CreateIndexResponse CreateIndex();

        BulkResponse BulkIndex(IEnumerable<Article> articles);

        ISearchResponse<Article> Search(string query);
    }

    public class ElasticsearchClient : IElasticsearchClient
    {
        public readonly IElasticClient Client;
        public readonly string IndexName;

        public ElasticsearchClient(IElasticClient client, string indexName)
        {
            IndexName = indexName;
            Client = client;
        }

        public ElasticsearchClient(Uri uri, string indexName)
            : this(CreateClient(uri), indexName)
        {
        }

        public CreateIndexResponse CreateIndex()
        {
            var response = Client.Indices.Exists(IndexName);

            if (response.Exists)
            {
                return null;
            }

            return Client.Indices.Create(IndexName, index => index.Map<Article>(ms => ms.AutoMap()));
        }

        public BulkResponse BulkIndex(IEnumerable<Article> articles)
        {
            var request = new BulkDescriptor();

            foreach (var article in articles)
            {
                request.Index<Article>(op => op
                    .Id(article.Id)
                    .Index(IndexName)
                    .Document(article));
            }

            return Client.Bulk(request);
        }

        public ISearchResponse<Article> Search(string query)
        {
            return Client.Search<Article>(x => x
                // Query this Index:
                .Index(IndexName)
                // Highlight Text Content:
                .Highlight(h => h
                    .Fields(x => x
                        .Field(x => x.Content)))
                // Now kick off the query:
                .Query(q => BuildQueryContainer(query)));
        }

        public ISearchResponse<Article> Suggest(string query)
        {
            return Client.Search<Article>(x => x
                // Query this Index:
                .Index(IndexName)
                // Suggest Titles:
                .Suggest(s => s.Term("suggestion", t => t
                    .Field(x => x.Title)
                    .Size(5)
                    .Text(query))));
        }

        private QueryContainer BuildQueryContainer(string query)
        {
            return Query<Article>.MultiMatch(x => x.Query(query)
                .Type(TextQueryType.BestFields)
                .Fields(f => f
                    .Field(x => x.Title, 2)
                    .Field(x => x.Content, 1)));
        }

        private static IElasticClient CreateClient(Uri uri)
        {
            var connectionPool = new SingleNodeConnectionPool(uri);
            var connectionSettings = new ConnectionSettings(connectionPool);

            return new ElasticClient(connectionSettings);
        }
    }
}
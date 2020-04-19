// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Utils;
using ElsevierFulltextApi;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Controllers
{
    public class IndexController : Controller
    {
        private readonly IElasticsearchClient elasticsearchClient;
        private readonly IElsevierFulltextApiClient elsevierClient;

        public IndexController(IElsevierFulltextApiClient elsevierClient, IElasticsearchClient elasticsearchClient)
        {
            this.elsevierClient = elsevierClient;
            this.elasticsearchClient = elasticsearchClient;
        }

        [HttpPut]
        [Route("/api/index")]
        public async Task<IActionResult> Query([FromQuery(Name = "doi")] string[] identifiers)
        {
            var articles = await GetArticles(identifiers);

            elasticsearchClient.BulkIndex(articles);

            return NoContent();
        }

        private async Task<List<Elasticsearch.Model.Article>> GetArticles(string[] identifiers)
        {
            var articles = new List<Elasticsearch.Model.Article>();
            
            foreach(var identifier in identifiers)
            {
                var fulltext = await elsevierClient.GetArticleByDOI(identifier);
                var article = ConvertToArticle(fulltext);
                
                articles.Add(article);
            }

            return articles;
        }

        private Elasticsearch.Model.Article ConvertToArticle(ElsevierFulltextApi.Model.FullText fulltext)
        {
            // Well this is how the Structure of the Elsevier XML looks... nothing to refactor here:
            var doi = fulltext?.OriginalText?.Doc?.SerialItem?.Article.Info?.DOI;
            var title = fulltext?.OriginalText?.Doc?.SerialItem?.Article?.Head?.Title;
            var sections = fulltext?.OriginalText?.Doc?.SerialItem?.Article?.Body?.Sections;
            var content = ElsevierClientUtils.GetContent(sections);

            return new Elasticsearch.Model.Article
            {
                Id = doi,
                Title = title,
                Content = content
            };
        }

    }
}
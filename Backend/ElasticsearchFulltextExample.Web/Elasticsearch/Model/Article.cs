
using Nest;

namespace ElasticsearchFulltextExample.Web.Elasticsearch.Model
{

    public class Article
    {
        [Text]
        public string Id { get; set; }

        [Text]
        public string Title { get; set; }

        [Text]
        public string Content { get; set; }
    }
}

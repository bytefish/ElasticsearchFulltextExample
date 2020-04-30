
using Nest;

namespace ElasticsearchFulltextExample.Web.Elasticsearch.Model
{
    public class Document
    {
        public string Id { get; set; }

        public string Content { get; set; }

        public Attachment Attachment { get; set; }
    }
}

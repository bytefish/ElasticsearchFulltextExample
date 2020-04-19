using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
{
    public class IndexDocumentDto
    {
        [JsonPropertyName("doi")]
        public string DOI { get; set; }
    }
}

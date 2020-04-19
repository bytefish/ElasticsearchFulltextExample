using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
{
    public class SearchSuggestionDto
    {
        [JsonPropertyName("query")]
        public string Query { get; set; }

        [JsonPropertyName("results")]
        public string[] Results { get; set; }
    }
}

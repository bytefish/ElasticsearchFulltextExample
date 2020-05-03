using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
{
    public class SearchSuggestionDto
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }


        [JsonPropertyName("highlight")]
        public string Highlight { get; set; }
    }
}

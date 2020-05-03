using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
{
    public class SearchSuggestionsDto
    {
        [JsonPropertyName("query")]
        public string Query { get; set; }


        [JsonPropertyName("results")]
        public SearchSuggestionDto[] Results { get; set; }
    }
}

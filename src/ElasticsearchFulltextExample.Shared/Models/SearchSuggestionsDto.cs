using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
{
    public class SearchSuggestionsDto
    {
        [JsonPropertyName("query")]
        public required string Query { get; set; }


        [JsonPropertyName("results")]
        public List<SearchSuggestionDto> Results { get; set; } = [];
    }
}

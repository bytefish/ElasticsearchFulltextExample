using System;
using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
{
    public class AttachmentDto
    {
        [JsonPropertyName("date")]
        public DateTime? Date { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("name")]
        public string Name  { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("keywords")]
        public string Keywords { get; set; }

        [JsonPropertyName("content-type")]
        public string ContentType { get; set; }

        [JsonPropertyName("content-length")]
        public int? ContentLength { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }
    }
}

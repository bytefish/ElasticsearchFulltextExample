using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
{
    public class DocumentDto
    {
        /// <summary>
        /// A unique document id.
        /// </summary>
        [JsonPropertyName("documentId")]
        public string DocumentId { get; set; }

        /// <summary>
        /// The Title of the Document for Suggestion.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// The Original Filename of the uploaded document.
        /// </summary>
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        /// <summary>
        /// The Data of the Document.
        /// </summary>
        [JsonPropertyName("data")]
        public byte[] Data { get; set; }

        /// <summary>
        /// Keywords to filter for.
        /// </summary>
        [JsonPropertyName("keywords")]
        public string[] Keywords { get; set; }

        /// <summary>
        /// Suggestions for the Autocomplete Field.
        /// </summary>
        [JsonPropertyName("suggestions")]
        public string[] Suggestions { get; set; }

        /// <summary>
        /// OCR Data.
        /// </summary>
        [JsonPropertyName("isOcrRequested")]
        public bool IsOcrRequested { get; set; }

        /// <summary>
        /// The Document Status.
        /// </summary>
        [JsonPropertyName("status")]
        public StatusEnumDto Status { get; set; }
    }
}

// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Outbox.Messages
{
    /// <summary>
    /// A Document has been created.
    /// </summary>
    public class DocumentCreatedMessage
    {
        /// <summary>
        /// Gets or sets the Document ID.
        /// </summary>
        [JsonPropertyName("documentId")]
        public int DocumentId { get; set; }
    }
}

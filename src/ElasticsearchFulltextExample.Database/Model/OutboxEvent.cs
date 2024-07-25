// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace ElasticsearchFulltextExample.Database.Model
{
    /// <summary>
    /// Outbox Events.
    /// </summary>
    public class OutboxEvent : Entity
    {
        /// <summary>
        /// Gets or sets an optional Correlation ID.
        /// </summary>
        public string? CorrelationId1 { get; set; }

        /// <summary>
        /// Gets or sets an optional Correlation ID.
        /// </summary>
        public string? CorrelationId2 { get; set; }

        /// <summary>
        /// Gets or sets an optional Correlation ID.
        /// </summary>
        public string? CorrelationId3 { get; set; }

        /// <summary>
        /// Gets or sets an optional Correlation ID.
        /// </summary>
        public string? CorrelationId4 { get; set; }

        /// <summary> 
        /// Gets or sets the type Event. 
        /// </summary>
        public required string EventType { get; set; }

        /// <summary> 
        /// Gets or sets the source of the event. 
        /// </summary>
        public string EventSource { get; set; } = "FTS";

        /// <summary> 
        /// The time (in UTC) the event was generated. 
        /// </summary>
        public DateTimeOffset EventTime { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the Events Payload.
        /// </summary>
        public required JsonDocument Payload { get; set; }
    }
}
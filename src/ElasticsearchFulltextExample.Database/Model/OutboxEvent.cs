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
        /// A Correlation ID.
        /// </summary>
        public string? CorrelationId1 { get; set; }

        /// <summary>
        /// A Correlation ID.
        /// </summary>
        public string? CorrelationId2 { get; set; }

        /// <summary>
        /// A Correlation ID.
        /// </summary>
        public string? CorrelationId3 { get; set; }

        /// <summary>
        /// A Correlation ID.
        /// </summary>
        public string? CorrelationId4 { get; set; }

        /// <summary> 
        /// The type of the event that occurred. 
        /// </summary>
        public required string EventType { get; set; }

        /// <summary> 
        /// The source the event occurred from. 
        /// </summary>
        public string EventSource { get; set; } = "FTS";

        /// <summary> 
        /// The time (in UTC) the event was generated. 
        /// </summary>
        public DateTimeOffset EventTime { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The Payload the Outbox event has.
        /// </summary>
        public required JsonDocument Payload { get; set; }
    }
}
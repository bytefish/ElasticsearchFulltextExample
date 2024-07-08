namespace ElasticsearchFulltextExample.Database.Model
{
    public class Job : Entity
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
        /// Gets or sets the DocumentId.
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the Creation Date.
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the Job Status.
        /// </summary>
        public JobStatusEnum JobStatusEnum { get; set; }

        /// <summary>
        /// Gets or sets the Scheduled Date.
        /// </summary>
        public DateTime ScheduledDate { get; set; } = DateTime.UtcNow;
    }
}
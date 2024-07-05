namespace ElasticsearchFulltextExample.Database.Model
{
    public class Job
    {
        /// <summary>
        /// Gets or sets the DocumentId.
        /// </summary>
        public int DocumentId { get; set; }

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
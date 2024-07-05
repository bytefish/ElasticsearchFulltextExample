// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Database.Model
{
    /// <summary>
    /// Defines all possible states a document can have.
    /// </summary>
    public enum JobStatusEnum
    {
        /// <summary>
        /// No Status.
        /// </summary>
        None = 0,

        /// <summary>
        /// Scheduled.
        /// </summary>
        Scheduled = 1,

        /// <summary>
        /// Executing.
        /// </summary>
        Executing = 2,
        
        /// <summary>
        /// Executing.
        /// </summary>
        Paused = 2,

        /// <summary>
        /// Finished.
        /// </summary>
        Finished = 3,

        /// <summary>
        /// Failed.
        /// </summary>
        Failed = 4,

        /// <summary>
        /// Cancelled.
        /// </summary>
        Cancelled = 5
    }
}

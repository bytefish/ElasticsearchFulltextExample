// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Web.Database.Model
{
    /// <summary>
    /// Defines all possible states a document can have.
    /// </summary>
    public enum StatusEnum
    {
        /// <summary>
        /// The document has no Status assigned.
        /// </summary>
        None = 0,

        /// <summary>
        /// The document is scheduled for indexing by a BackgroundService.
        /// </summary>
        ScheduledIndex = 1,

        /// <summary>
        /// The document is scheduled for deletion by a BackgroundService.
        /// </summary>
        ScheduledDelete = 2,

        /// <summary>
        /// The document has been indexed.
        /// </summary>
        Indexed = 3,

        /// <summary>
        /// The document indexing has failed due to an error.
        /// </summary>
        Failed = 4,

        /// <summary>
        /// The document has been removed from the index.
        /// </summary>
        Deleted = 5
    }
}

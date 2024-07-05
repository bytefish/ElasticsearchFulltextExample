// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Api.Database.Model
{
    /// <summary>
    /// Defines all possible states a document can have.
    /// </summary>
    public enum ProcessingStatusEnum
    {
        /// <summary>
        /// No Status.
        /// </summary>
        None = 0,

        /// <summary>
        /// Queued.
        /// </summary>
        Queued = 1,

        /// <summary>
        /// Processing.
        /// </summary>
        Processing = 2,

        /// <summary>
        /// Indexed.
        /// </summary>
        Indexed = 3,

        /// <summary>
        /// Failed.
        /// </summary>
        Failed = 4,

        /// <summary>
        /// Deleted.
        /// </summary>
        Deleted = 5
    }
}

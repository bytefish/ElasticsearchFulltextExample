// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Web.Database.Model
{
    public enum StatusEnum
    {
        None = 0,
        ScheduledIndex = 1,
        ScheduledDelete = 2,
        Indexed = 3,
        Failed = 4,
        Deleted = 5
    }
}

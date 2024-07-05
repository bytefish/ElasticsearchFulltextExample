// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusEnumDto
    {
        None = 0,
        ScheduledIndex = 1,
        ScheduledDelete = 2,
        Indexed = 3,
        Failed = 4,
        Deleted = 5
    }
}

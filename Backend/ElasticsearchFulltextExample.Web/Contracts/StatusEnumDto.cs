// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Web.Contracts
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusEnumDto
    {
        None = 0,
        Scheduled = 1,
        Indexed = 2,
        Failed = 3
    }
}

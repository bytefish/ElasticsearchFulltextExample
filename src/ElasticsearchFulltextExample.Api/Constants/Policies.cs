﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Api.Constants
{
    /// <summary>
    /// Authorization Policies.
    /// </summary>
    public class Policies
    {
        /// <summary>
        /// Requires the User Role to be set.
        /// </summary>
        public const string RequireUserRole = "RequireUserRole";

        /// <summary>
        /// Required the Admin Role to be set.
        /// </summary>
        public const string RequireAdminRole = "RequireAdminRole";

        /// <summary>
        /// Per-User Rate Limiting Policy.
        /// </summary>
        public const string PerUserRatelimit = "PerUserRatelimit";
    }
}

﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Database.Model
{
    public class User : Entity
    {
        /// <summary>
        /// Gets or sets the Email.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Gets or sets the PreferredName.
        /// </summary>
        public required string PreferredName { get; set; }
    }
}

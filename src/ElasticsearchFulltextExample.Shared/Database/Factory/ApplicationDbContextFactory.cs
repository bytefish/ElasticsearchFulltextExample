// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ElasticsearchFulltextExample.Web.Database.Factory
{
    public class ApplicationDbContextFactory
    {
        private readonly DbContextOptions options;

        public ApplicationDbContextFactory(DbContextOptions options)
        {
            this.options = options;
        }

        public ApplicationDbContext Create()
        {
            return new ApplicationDbContext(options);
        }
    }
}

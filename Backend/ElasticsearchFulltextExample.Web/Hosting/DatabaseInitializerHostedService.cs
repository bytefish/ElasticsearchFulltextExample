// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Database.Factory;
using ElasticsearchFulltextExample.Web.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Hosting
{
    public class DatabaseInitializerHostedService : BackgroundService
    {
        private readonly ILogger<DatabaseInitializerHostedService> logger;
        private readonly ApplicationDbContextFactory applicationDbContextFactory;

        public DatabaseInitializerHostedService(ILogger<DatabaseInitializerHostedService> logger, ApplicationDbContextFactory applicationDbContextFactory)
        {
            this.logger = logger;
            this.applicationDbContextFactory = applicationDbContextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (var context = applicationDbContextFactory.Create())
            {
                await context.Database.MigrateAsync();
            }
        }
    }
}
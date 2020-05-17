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
            this.applicationDbContextFactory = applicationDbContextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var pingDelay = TimeSpan.FromSeconds(5);

            // We have to wait until the Posgres Cluster is up and ready:
            while (!await IsServerReachableAsync(cancellationToken))
            {
                if (logger.IsWarningEnabled())
                {
                    logger.LogWarning($"Postgres is not reachable. Retrying in {pingDelay.Seconds} seconds ...");
                }

                await Task.Delay(pingDelay, cancellationToken);
            }

            using (var context = applicationDbContextFactory.Create())
            {
                await context.Database.MigrateAsync();
            }
        }

        public async Task<bool> IsServerReachableAsync(CancellationToken cancellationToken)
        {
            using(var context = applicationDbContextFactory.Create())
            {
                return await context.Database.CanConnectAsync(cancellationToken);
            }
        }
    }
}
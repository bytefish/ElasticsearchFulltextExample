// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Database.Factory;
using ElasticsearchFulltextExample.Web.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Hosting
{
    public class DatabaseInitializerHostedService : IHostedService
    {
        private readonly ILogger<DatabaseInitializerHostedService> logger;
        private readonly ApplicationDbContextFactory applicationDbContextFactory;

        public DatabaseInitializerHostedService(ILogger<DatabaseInitializerHostedService> logger, ApplicationDbContextFactory applicationDbContextFactory)
        {
            this.logger = logger;
            this.applicationDbContextFactory = applicationDbContextFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if(logger.IsInformationEnabled())
            {
                logger.LogInformation("Start Database Migration ...");
            }

            using (var context = applicationDbContextFactory.Create())
            {
                await context.Database.MigrateAsync();
            }

            if (logger.IsInformationEnabled())
            {
                logger.LogInformation("Finish Database Migration ...");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
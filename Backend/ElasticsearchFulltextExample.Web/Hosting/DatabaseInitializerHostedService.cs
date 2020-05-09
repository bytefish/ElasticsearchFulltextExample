// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Database.Factory;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Hosting
{
    public class DatabaseInitializerHostedService : IHostedService
    {
        private readonly ApplicationDbContextFactory applicationDbContextFactory;

        public DatabaseInitializerHostedService(ApplicationDbContextFactory applicationDbContextFactory)
        {
            this.applicationDbContextFactory = applicationDbContextFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var context = applicationDbContextFactory.Create())
            {
                await context.Database.EnsureCreatedAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
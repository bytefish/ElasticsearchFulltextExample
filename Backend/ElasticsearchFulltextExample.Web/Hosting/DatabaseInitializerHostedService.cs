using ElasticsearchFulltextExample.Web.Database.Context;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Hosting
{
    public class DatabaseInitializerHostedService : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using(var context = new ApplicationDbContext())
            {
                await context.Database.EnsureCreatedAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

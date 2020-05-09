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

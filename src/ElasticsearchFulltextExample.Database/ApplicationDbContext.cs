// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace ElasticsearchFulltextExample.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options) { }

        public DbSet<Document> Documents { get; set; }

        public DbSet<Job> Jobs { get; set; }
        
        public DbSet<JobStatus> JobStatus { get; set; }

        public DbSet<Suggestion> Suggestions { get; set; }

        public DbSet<Keyword> Keywords { get; set; }
        
        public DbSet<DocumentKeyword> DocumentKeywords { get; set; }

        public DbSet<DocumentSuggestion> DocumentSuggestions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
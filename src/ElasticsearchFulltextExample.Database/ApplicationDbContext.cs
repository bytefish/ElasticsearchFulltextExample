// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace ElasticsearchFulltextExample.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        
        public DbSet<Document> Documents { get; set; }

        public DbSet<Suggestion> Suggestions { get; set; }

        public DbSet<Keyword> Keywords { get; set; }
        
        public DbSet<DocumentKeyword> DocumentKeywords { get; set; }

        public DbSet<DocumentSuggestion> DocumentSuggestions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Sequences
            modelBuilder.HasSequence<int>("outbox_event_seq", schema: "fts")
                .StartsAt(38187)
                .IncrementsBy(1);

            modelBuilder.HasSequence<int>("document_seq", schema: "fts")
                .StartsAt(38187)
                .IncrementsBy(1);

            modelBuilder.HasSequence<int>("document_keyword_seq", schema: "fts")
                .StartsAt(38187)
                .IncrementsBy(1);

            modelBuilder.HasSequence<int>("document_suggestion_seq", schema: "fts")
                .StartsAt(38187)
                .IncrementsBy(1);

            modelBuilder.HasSequence<int>("keyword_seq", schema: "fts")
                .StartsAt(38187)
                .IncrementsBy(1);

            modelBuilder.HasSequence<int>("suggestion_seq", schema: "fts")
                .StartsAt(38187)
                .IncrementsBy(1);

            modelBuilder.HasSequence<int>("user_seq", schema: "fts")
                .StartsAt(38187)
                .IncrementsBy(1);

            // Tables
            modelBuilder.Entity<OutboxEvent>(entity =>
            {
                entity.ToTable("outbox_event", "fts");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("INT")
                    .HasColumnName("outbox_event_id")
                    .UseHiLo("user_seq", "fts")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CorrelationId1)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("correlation_id_1")
                    .HasMaxLength(2000)
                    .IsRequired(false);

                entity.Property(e => e.CorrelationId2)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("correlation_id_2")
                    .HasMaxLength(2000)
                    .IsRequired(false);

                entity.Property(e => e.CorrelationId3)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("correlation_id_3")
                    .HasMaxLength(2000)
                    .IsRequired(false);

                entity.Property(e => e.CorrelationId4)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("correlation_id_4")
                    .HasMaxLength(2000)
                    .IsRequired(false);

                entity.Property(e => e.EventTime)
                    .HasColumnType("timestamptz")
                    .HasColumnName("event_time")
                    .IsRequired(true);

                entity.Property(e => e.EventType)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("event_type")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.EventSource)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("event_source")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.Payload)
                    .HasColumnType("jsonb")
                    .HasColumnName("payload")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.SysPeriod)
                    .HasColumnType("tstzrange")
                    .HasColumnName("sys_period")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user", "fts");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("INT")
                    .HasColumnName("user_id")
                    .UseHiLo("user_seq", "fts")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Email)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("email")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.PreferredName)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("preferred_name")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.SysPeriod)
                    .HasColumnType("tstzrange")
                    .HasColumnName("sys_period")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("document", "fts");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .UseHiLo("user_seq", "fts")
                    .HasColumnName("document_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("title")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.Filename)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("filename")
                    .HasMaxLength(2000)
                    .IsRequired(true);
                
                entity.Property(e => e.Data)
                    .HasColumnType("bytea")
                    .HasColumnName("data")
                    .IsRequired(true);

                entity.Property(e => e.UploadedAt)
                    .HasColumnType("timestamptz")
                    .HasColumnName("uploaded_at")
                    .IsRequired(true);
                
                entity.Property(e => e.IndexedAt)
                    .HasColumnType("timestamptz")
                    .HasColumnName("indexed_at")
                    .IsRequired(false);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.SysPeriod)
                    .HasColumnType("tstzrange")
                    .HasColumnName("sys_period")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity
                    .HasMany<DocumentSuggestion>()
                    .WithOne();

                entity
                    .HasMany<DocumentKeyword>()
                    .WithOne();
            });

            modelBuilder.Entity<Keyword>(entity =>
            {
                entity.ToTable("keyword", "fts");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("INT")
                    .UseHiLo("keyword_seq", "fts")
                    .HasColumnName("keyword_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(255)")
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.SysPeriod)
                    .HasColumnType("tstzrange")
                    .HasColumnName("sys_period")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity
                    .HasMany<DocumentKeyword>()
                    .WithOne();
            });

            modelBuilder.Entity<Suggestion>(entity =>
            {
                entity.ToTable("suggestion", "fts");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("INT")
                    .UseHiLo("suggestion_seq", "fts")
                    .HasColumnName("suggestion_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(255)")
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.SysPeriod)
                    .HasColumnType("tstzrange")
                    .HasColumnName("sys_period")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity
                    .HasMany<DocumentSuggestion>()
                    .WithOne();
            });

            modelBuilder.Entity<DocumentKeyword>(entity =>
            {
                entity.ToTable("document_keyword", "fts");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .HasColumnName("document_keyword_id")
                    .UseHiLo("document_keyword_seq", "fts")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.DocumentId)
                    .HasColumnType("int")
                    .HasColumnName("document_id")
                    .IsRequired(true);

                entity.Property(e => e.KeywordId)
                    .HasColumnType("int")
                    .HasColumnName("keyword_id")
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.SysPeriod)
                    .HasColumnType("tstzrange")
                    .HasColumnName("sys_period")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity
                    .HasOne<Keyword>()
                    .WithMany()
                    .HasForeignKey(x => x.KeywordId);

                entity
                    .HasOne<Document>()
                    .WithMany()
                    .HasForeignKey(x => x.DocumentId);
            });

            modelBuilder.Entity<DocumentSuggestion>(entity =>
            {
                entity.ToTable("document_suggestion", "fts");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .HasColumnName("document_suggestion_id")
                    .UseHiLo("document_suggestion_seq", "fts")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.DocumentId)
                    .HasColumnType("int")
                    .HasColumnName("document_id")
                    .IsRequired(true);

                entity.Property(e => e.SuggestionId)
                    .HasColumnType("int")
                    .HasColumnName("suggestion_id")
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.SysPeriod)
                    .HasColumnType("tstzrange")
                    .HasColumnName("sys_period")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity
                    .HasOne<Suggestion>()
                    .WithMany()
                    .HasForeignKey(x => x.SuggestionId);


                entity
                    .HasOne<Document>()
                    .WithMany()
                    .HasForeignKey(x => x.DocumentId);

            });

            // History Tables

            base.OnModelCreating(modelBuilder);
        }
    }
}
// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Database.Model;
using ElasticsearchFulltextExample.Web.Database.ValueComparers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElasticsearchFulltextExample.Web.Database.TypeConfigurations
{
    public class DocumentTypeConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder
                .ToTable("documents")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder
                .Property(x => x.Filename)
                .HasColumnName("filename")
                .IsRequired();

            builder
                .Property(x => x.Title)
                .HasColumnName("title")
                .IsRequired();

            builder
                .Property(x => x.Data)
                .HasColumnName("data")
                .IsRequired();

            builder
                .Property(x => x.Suggestions)
                .HasColumnName("suggestions")
                .HasConversion(new DelimitedStringValueConverter(','))
                .IsRequired()
                .Metadata.SetValueComparer(new StringArrayValueComparer());

            builder
                .Property(x => x.Keywords)
                .HasColumnName("keywords")
                .HasConversion(new DelimitedStringValueConverter(','))
                .IsRequired()
                .Metadata.SetValueComparer(new StringArrayValueComparer());

            builder
                .Property(x => x.IsOcrRequested)
                .HasColumnName("is_ocr_requested")
                .IsRequired();

            builder
                .Property(x => x.UploadedAt)
                .HasColumnName("uploaded_at")
                .IsRequired();

            builder
                .Property(x => x.IndexedAt)
                .HasColumnName("indexed_at");

            builder
                .Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<int>()
                .IsRequired();
        }
    }
}

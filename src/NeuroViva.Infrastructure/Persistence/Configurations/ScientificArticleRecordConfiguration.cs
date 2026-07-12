using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Content;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class ScientificArticleRecordConfiguration : IEntityTypeConfiguration<ScientificArticleRecord>
{
    public void Configure(EntityTypeBuilder<ScientificArticleRecord> builder)
    {
        builder.ToTable("scientific_article_record");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.DiseaseId).HasColumnName("disease_id").IsRequired();
        builder.Property(a => a.Title).HasColumnName("title").IsRequired();
        builder.Property(a => a.SourceName).HasColumnName("source_name");
        builder.Property(a => a.SourceUrl).HasColumnName("source_url").IsRequired();
        builder.Property(a => a.Description).HasColumnName("description");
        builder.Property(a => a.Authors).HasColumnName("authors");
        builder.Property(a => a.PublishedAt).HasColumnName("published_at");
        builder.Property(a => a.FetchedAt).HasColumnName("fetched_at");
        builder.Property(a => a.ExternalGuid).HasColumnName("external_guid").IsRequired();
        builder.Property(a => a.Language).HasColumnName("language").IsRequired().HasMaxLength(2);
        builder.HasIndex(a => new { a.DiseaseId, a.ExternalGuid })
            .IsUnique()
            .HasDatabaseName("ix_scientific_article_record_disease_external_guid");
        builder.Ignore(a => a.DomainEvents);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Content;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class NewsArticleConfiguration : IEntityTypeConfiguration<NewsArticle>
{
    public void Configure(EntityTypeBuilder<NewsArticle> builder)
    {
        builder.ToTable("news_article");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.DiseaseId).HasColumnName("disease_id").IsRequired();
        builder.Property(a => a.Title).HasColumnName("title").IsRequired();
        builder.Property(a => a.SourceName).HasColumnName("source_name");
        builder.Property(a => a.SourceUrl).HasColumnName("source_url").IsRequired();
        builder.Property(a => a.Description).HasColumnName("description");
        builder.Property(a => a.PublishedAt).HasColumnName("published_at");
        builder.Property(a => a.FetchedAt).HasColumnName("fetched_at");
        builder.Property(a => a.ExternalGuid).HasColumnName("external_guid").IsRequired();
        builder.HasIndex(a => new { a.DiseaseId, a.ExternalGuid })
            .IsUnique()
            .HasDatabaseName("ix_news_article_disease_external_guid");
        builder.Ignore(a => a.DomainEvents);
    }
}

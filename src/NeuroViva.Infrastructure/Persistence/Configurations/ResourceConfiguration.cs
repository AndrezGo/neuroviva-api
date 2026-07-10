using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Content;
using NeuroViva.Domain.Content.Enums;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    // recurso_tipo_check: news, scientific_article, video
    private static string TypeToDb(ResourceType v)
    {
        if (v == ResourceType.News)              return "news";
        if (v == ResourceType.ScientificArticle) return "scientific_article";
        if (v == ResourceType.Video)             return "video";
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unmapped ResourceType value.");
    }

    private static ResourceType TypeFromDb(string v)
    {
        if (v == "news")               return ResourceType.News;
        if (v == "scientific_article") return ResourceType.ScientificArticle;
        if (v == "video")              return ResourceType.Video;
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unknown ResourceType DB value.");
    }

    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("resource");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.AuthorId).HasColumnName("author_id");
        builder.Property(r => r.DiseaseId).HasColumnName("disease_id");
        builder.Property(r => r.Title).HasColumnName("title");
        builder.Property(r => r.Type).HasColumnName("type")
            .HasConversion(v => TypeToDb(v), v => TypeFromDb(v));
        builder.Property(r => r.Url).HasColumnName("url");
        builder.Property(r => r.Description).HasColumnName("description");
        builder.Property(r => r.ApprovalStatus).HasColumnName("approval_status");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Ignore(r => r.DomainEvents);
    }
}

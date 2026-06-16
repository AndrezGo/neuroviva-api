using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Content;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("resource");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.AuthorId).HasColumnName("author_id");
        builder.Property(r => r.DiseaseId).HasColumnName("disease_id");
        builder.Property(r => r.Title).HasColumnName("title");
        builder.Property(r => r.Type).HasColumnName("type");
        builder.Property(r => r.Url).HasColumnName("url");
        builder.Property(r => r.Description).HasColumnName("description");
        builder.Property(r => r.ApprovalStatus).HasColumnName("approval_status");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Ignore(r => r.DomainEvents);
    }
}

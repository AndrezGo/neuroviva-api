using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Community;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class CommunityPostConfiguration : IEntityTypeConfiguration<CommunityPost>
{
    public void Configure(EntityTypeBuilder<CommunityPost> builder)
    {
        builder.ToTable("community_post");
        builder.HasKey(cp => cp.Id);
        builder.Property(cp => cp.Id).HasColumnName("id");
        builder.Property(cp => cp.AuthorId).HasColumnName("author_id");
        builder.Property(cp => cp.PatientId).HasColumnName("patient_id");
        builder.Property(cp => cp.DiseaseId).HasColumnName("disease_id");
        builder.Property(cp => cp.Content).HasColumnName("content");
        builder.Property(cp => cp.Visibility).HasColumnName("visibility");
        builder.Property(cp => cp.CreatedAt).HasColumnName("created_at");
        builder.Ignore(cp => cp.DomainEvents);
    }
}

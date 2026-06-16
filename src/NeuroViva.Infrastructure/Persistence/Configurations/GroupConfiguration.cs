using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Community;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("group");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id");
        builder.Property(g => g.CreatorId).HasColumnName("creator_id");
        builder.Property(g => g.DiseaseId).HasColumnName("disease_id");
        builder.Property(g => g.Name).HasColumnName("name");
        builder.Property(g => g.Slug).HasColumnName("slug");
        builder.Property(g => g.Description).HasColumnName("description");
        builder.Property(g => g.AvatarUrl).HasColumnName("avatar_url");
        builder.Property(g => g.Visibility).HasColumnName("visibility");
        builder.Property(g => g.Active).HasColumnName("active");
        builder.Property(g => g.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(g => g.Slug).IsUnique();
        builder.Ignore(g => g.DomainEvents);
    }
}

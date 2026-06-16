using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Community;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class CommunityReactionConfiguration : IEntityTypeConfiguration<CommunityReaction>
{
    public void Configure(EntityTypeBuilder<CommunityReaction> builder)
    {
        builder.ToTable("community_reaction");
        builder.HasKey(cr => cr.Id);
        builder.Property(cr => cr.Id).HasColumnName("id");
        builder.Property(cr => cr.PostId).HasColumnName("post_id");
        builder.Property(cr => cr.UserId).HasColumnName("user_id");
        builder.Property(cr => cr.Type).HasColumnName("type");
        builder.Property(cr => cr.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(cr => new { cr.PostId, cr.UserId, cr.Type }).IsUnique();
    }
}

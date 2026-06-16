using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Community;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class CommunityCommentConfiguration : IEntityTypeConfiguration<CommunityComment>
{
    public void Configure(EntityTypeBuilder<CommunityComment> builder)
    {
        builder.ToTable("community_comment");
        builder.HasKey(cc => cc.Id);
        builder.Property(cc => cc.Id).HasColumnName("id");
        builder.Property(cc => cc.PostId).HasColumnName("post_id");
        builder.Property(cc => cc.AuthorId).HasColumnName("author_id");
        builder.Property(cc => cc.Content).HasColumnName("content");
        builder.Property(cc => cc.CreatedAt).HasColumnName("created_at");
    }
}

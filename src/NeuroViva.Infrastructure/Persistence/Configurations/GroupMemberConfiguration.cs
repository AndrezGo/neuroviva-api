using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Community;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("group_member");
        builder.HasKey(gm => gm.Id);
        builder.Property(gm => gm.Id).HasColumnName("id");
        builder.Property(gm => gm.GroupId).HasColumnName("group_id");
        builder.Property(gm => gm.UserId).HasColumnName("user_id");
        builder.Property(gm => gm.Role).HasColumnName("rol");
        builder.Property(gm => gm.Muted).HasColumnName("muted");
        builder.Property(gm => gm.MutedUntil).HasColumnName("muted_until");
        builder.Property(gm => gm.Status).HasColumnName("status");
        builder.Property(gm => gm.JoinedAt).HasColumnName("joined_at");
        builder.HasIndex(gm => new { gm.GroupId, gm.UserId }).IsUnique();
    }
}

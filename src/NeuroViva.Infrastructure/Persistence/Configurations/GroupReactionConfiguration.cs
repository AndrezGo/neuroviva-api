using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Community;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class GroupReactionConfiguration : IEntityTypeConfiguration<GroupReaction>
{
    public void Configure(EntityTypeBuilder<GroupReaction> builder)
    {
        builder.ToTable("group_reaction");
        builder.HasKey(gr => gr.Id);
        builder.Property(gr => gr.Id).HasColumnName("id");
        builder.Property(gr => gr.MessageId).HasColumnName("message_id");
        builder.Property(gr => gr.UserId).HasColumnName("user_id");
        builder.Property(gr => gr.Emoji).HasColumnName("emoji");
        builder.Property(gr => gr.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(gr => new { gr.MessageId, gr.UserId, gr.Emoji }).IsUnique();
    }
}

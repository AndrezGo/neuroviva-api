using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Community;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class GroupMessageConfiguration : IEntityTypeConfiguration<GroupMessage>
{
    public void Configure(EntityTypeBuilder<GroupMessage> builder)
    {
        builder.ToTable("group_message");
        builder.HasKey(gm => gm.Id);
        builder.Property(gm => gm.Id).HasColumnName("id");
        builder.Property(gm => gm.GroupId).HasColumnName("group_id");
        builder.Property(gm => gm.AuthorId).HasColumnName("author_id");
        builder.Property(gm => gm.Content).HasColumnName("content");
        builder.Property(gm => gm.Type).HasColumnName("type");
        builder.Property(gm => gm.FileUrl).HasColumnName("file_url");
        builder.Property(gm => gm.ReplyTo).HasColumnName("reply_to");
        builder.Property(gm => gm.Deleted).HasColumnName("deleted");
        builder.Property(gm => gm.CreatedAt).HasColumnName("created_at");
        builder.Ignore(gm => gm.DomainEvents);
    }
}

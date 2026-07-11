using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Content;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("channel");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.Name).HasColumnName("name").IsRequired();
        builder.Property(c => c.Description).HasColumnName("description");
        builder.Property(c => c.AvatarUrl).HasColumnName("avatar_url");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Ignore(c => c.DomainEvents);
    }
}

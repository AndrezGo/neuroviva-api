using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Users;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.TenantId).HasColumnName("tenant_id");
        builder.Property(u => u.AuthUserId).HasColumnName("auth_user_id");
        builder.Property(u => u.Name).HasColumnName("name").IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").IsRequired();
        builder.Property(u => u.AvatarUrl).HasColumnName("avatar_url");
        builder.Property(u => u.IsActive).HasColumnName("active").HasDefaultValue(true);
        builder.Property(u => u.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(u => u.AuthUserId).IsUnique();
        builder.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
        builder.Ignore(u => u.DomainEvents);
    }
}

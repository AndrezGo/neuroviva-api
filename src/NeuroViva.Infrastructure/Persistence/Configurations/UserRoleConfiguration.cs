using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Users;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_role");
        builder.HasKey(ur => ur.Id);
        builder.Property(ur => ur.Id).HasColumnName("id");
        builder.Property(ur => ur.UserId).HasColumnName("user_id");
        builder.Property(ur => ur.RoleId).HasColumnName("role_id");
        builder.Property(ur => ur.AssignedAt).HasColumnName("assigned_at");
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Users;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("role");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.Name).HasColumnName("name").IsRequired();
        builder.Property(r => r.Description).HasColumnName("description");
        builder.HasIndex(r => r.Name).IsUnique();
    }
}

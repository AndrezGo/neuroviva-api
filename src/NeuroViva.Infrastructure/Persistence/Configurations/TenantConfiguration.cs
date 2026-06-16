using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Tenancy;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenant");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.Name).HasColumnName("nombre").IsRequired();
        builder.Property(t => t.Domain).HasColumnName("dominio");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(t => t.Domain).IsUnique();
        builder.Ignore(t => t.DomainEvents);
    }
}

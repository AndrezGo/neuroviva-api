using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Billing;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("payment_method");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.TenantId).HasColumnName("tenant_id");
        builder.Property(p => p.Type).HasColumnName("type");
        builder.Property(p => p.Last4).HasColumnName("last4");
        builder.Property(p => p.Brand).HasColumnName("brand");
        builder.Property(p => p.GatewayToken).HasColumnName("gateway_token");
        builder.Property(p => p.IsDefault).HasColumnName("is_default");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
    }
}

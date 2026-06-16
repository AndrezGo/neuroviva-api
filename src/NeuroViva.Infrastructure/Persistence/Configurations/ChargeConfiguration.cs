using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Billing;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class ChargeConfiguration : IEntityTypeConfiguration<Charge>
{
    public void Configure(EntityTypeBuilder<Charge> builder)
    {
        builder.ToTable("charge");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.SubscriptionId).HasColumnName("subscription_id");
        builder.Property(c => c.PaymentMethodId).HasColumnName("payment_method_id");
        builder.Property(c => c.Amount).HasColumnName("amount").HasPrecision(10, 2);
        builder.Property(c => c.Currency).HasColumnName("currency");
        builder.Property(c => c.Status).HasColumnName("status");
        builder.Property(c => c.GatewayReference).HasColumnName("gateway_reference");
        builder.Property(c => c.ChargedAt).HasColumnName("charged_at");
        builder.Property(c => c.NextChargeAt).HasColumnName("next_charge_at");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
    }
}

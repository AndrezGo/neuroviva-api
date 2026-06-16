using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Billing;
using NeuroViva.Domain.Billing.Enums;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscription");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.TenantId).HasColumnName("tenant_id");
        builder.Property(s => s.PlanId).HasColumnName("plan_id");
        builder.Property(s => s.Status).HasColumnName("status")
            .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<SubscriptionStatus>(v, true));
        builder.Property(s => s.TrialStart).HasColumnName("trial_start");
        builder.Property(s => s.TrialEnd).HasColumnName("trial_end");
        builder.Property(s => s.PeriodStart).HasColumnName("period_start");
        builder.Property(s => s.PeriodEnd).HasColumnName("period_end");
        builder.Property(s => s.CardRegistered).HasColumnName("card_registered");
        builder.Property(s => s.CardRegisteredAt).HasColumnName("card_registered_at");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(s => s.TenantId).IsUnique();
        builder.Ignore(s => s.DomainEvents);
    }
}

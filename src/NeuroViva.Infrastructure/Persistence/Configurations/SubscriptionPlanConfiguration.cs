using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Billing;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plan");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Name).HasColumnName("nombre").IsRequired();
        builder.Property(p => p.Description).HasColumnName("descripcion");
        builder.Property(p => p.MonthlyPrice).HasColumnName("monthly_price").HasPrecision(10, 2);
        builder.Property(p => p.TrialDays).HasColumnName("trial_days");
        builder.Property(p => p.Active).HasColumnName("active");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
    }
}

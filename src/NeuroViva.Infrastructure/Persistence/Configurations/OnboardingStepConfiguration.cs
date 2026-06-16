using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Onboarding;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class OnboardingStepConfiguration : IEntityTypeConfiguration<OnboardingStep>
{
    public void Configure(EntityTypeBuilder<OnboardingStep> builder)
    {
        builder.ToTable("onboarding_step");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.Role).HasColumnName("rol");
        builder.Property(s => s.OrderNum).HasColumnName("order_num");
        builder.Property(s => s.Type).HasColumnName("type");
        builder.Property(s => s.Title).HasColumnName("title");
        builder.Property(s => s.Description).HasColumnName("description");
        builder.Property(s => s.Options).HasColumnName("options").HasColumnType("jsonb");
        builder.Property(s => s.Skippable).HasColumnName("skippable");
        builder.HasIndex(s => new { s.Role, s.OrderNum }).IsUnique();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Onboarding;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class OnboardingSessionConfiguration : IEntityTypeConfiguration<OnboardingSession>
{
    public void Configure(EntityTypeBuilder<OnboardingSession> builder)
    {
        builder.ToTable("onboarding_session");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.UserId).HasColumnName("user_id");
        builder.Property(s => s.Role).HasColumnName("rol");
        builder.Property(s => s.CurrentStep).HasColumnName("current_step");
        builder.Property(s => s.TotalSteps).HasColumnName("total_steps");
        builder.Property(s => s.Completed).HasColumnName("completed");
        builder.Property(s => s.Answers).HasColumnName("answers").HasColumnType("jsonb");
        builder.Property(s => s.StartedAt).HasColumnName("started_at");
        builder.Property(s => s.CompletedAt).HasColumnName("completed_at");
        builder.HasIndex(s => new { s.UserId, s.Role }).IsUnique();
    }
}

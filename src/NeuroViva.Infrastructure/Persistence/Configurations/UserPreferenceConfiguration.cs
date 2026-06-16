using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Users;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("user_preference");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.UserId).HasColumnName("user_id");
        builder.Property(p => p.LargeText).HasColumnName("large_text");
        builder.Property(p => p.HighContrast).HasColumnName("high_contrast");
        builder.Property(p => p.NotifyMedications).HasColumnName("notify_medications");
        builder.Property(p => p.NotifyAppointments).HasColumnName("notify_appointments");
        builder.Property(p => p.Language).HasColumnName("language");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(p => p.UserId).IsUnique();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.HealthMonitoring;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class MoodLogConfiguration : IEntityTypeConfiguration<MoodLog>
{
    public void Configure(EntityTypeBuilder<MoodLog> builder)
    {
        builder.ToTable("mood_log");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.PatientId).HasColumnName("patient_id");
        builder.Property(m => m.LoggedBy).HasColumnName("logged_by");
        builder.Property(m => m.LevelValue).HasColumnName("level");
        builder.Property(m => m.Note).HasColumnName("note");
        builder.Property(m => m.LoggedAt).HasColumnName("logged_at");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Ignore(m => m.DomainEvents);
    }
}

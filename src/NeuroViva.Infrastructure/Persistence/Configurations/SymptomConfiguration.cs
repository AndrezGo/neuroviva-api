using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.HealthMonitoring;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class SymptomConfiguration : IEntityTypeConfiguration<Symptom>
{
    public void Configure(EntityTypeBuilder<Symptom> builder)
    {
        builder.ToTable("symptom");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.PatientId).HasColumnName("patient_id");
        builder.Property(s => s.LoggedBy).HasColumnName("logged_by");
        builder.Property(s => s.Type).HasColumnName("type");
        builder.Property(s => s.IntensityValue).HasColumnName("intensity");
        builder.Property(s => s.Description).HasColumnName("description");
        builder.Property(s => s.LoggedAt).HasColumnName("logged_at");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Ignore(s => s.DomainEvents);
    }
}

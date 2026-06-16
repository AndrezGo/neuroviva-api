using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Enums;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class ClinicalRecordConfiguration : IEntityTypeConfiguration<ClinicalRecord>
{
    public void Configure(EntityTypeBuilder<ClinicalRecord> builder)
    {
        builder.ToTable("clinical_record");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.PatientId).HasColumnName("patient_id");
        builder.Property(c => c.CreatedBy).HasColumnName("created_by");
        builder.Property(c => c.EventType).HasColumnName("event_type")
            .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<ClinicalEventType>(v, true));
        builder.Property(c => c.Description).HasColumnName("description");
        builder.Property(c => c.EventDate).HasColumnName("event_at");
        builder.Property(c => c.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
    }
}

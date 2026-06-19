using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Enums;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class ClinicalRecordConfiguration : IEntityTypeConfiguration<ClinicalRecord>
{
    // historia_clinica_tipo_evento_check: consulta, medicamento, sintoma, examen, nota, otro
    private static string EventTypeToDb(ClinicalEventType v)
    {
        if (v == ClinicalEventType.Consultation) return "consulta";
        if (v == ClinicalEventType.Medication)   return "medicamento";
        if (v == ClinicalEventType.Symptom)      return "sintoma";
        if (v == ClinicalEventType.Exam)         return "examen";
        if (v == ClinicalEventType.Note)         return "nota";
        if (v == ClinicalEventType.Other)        return "otro";
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unmapped ClinicalEventType value.");
    }

    private static ClinicalEventType EventTypeFromDb(string v)
    {
        if (v == "consulta")    return ClinicalEventType.Consultation;
        if (v == "medicamento") return ClinicalEventType.Medication;
        if (v == "sintoma")     return ClinicalEventType.Symptom;
        if (v == "examen")      return ClinicalEventType.Exam;
        if (v == "nota")        return ClinicalEventType.Note;
        if (v == "otro")        return ClinicalEventType.Other;
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unknown ClinicalEventType DB value.");
    }

    public void Configure(EntityTypeBuilder<ClinicalRecord> builder)
    {
        builder.ToTable("clinical_record");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.PatientId).HasColumnName("patient_id");
        builder.Property(c => c.CreatedBy).HasColumnName("created_by");
        builder.Property(c => c.EventType).HasColumnName("event_type")
            .HasConversion(v => EventTypeToDb(v), v => EventTypeFromDb(v));
        builder.Property(c => c.Description).HasColumnName("description");
        builder.Property(c => c.EventDate).HasColumnName("event_at");
        builder.Property(c => c.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
    }
}

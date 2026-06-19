using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Enums;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    // alerta_prioridad_check: info, media, alta, critica
    private static string PriorityToDb(AlertPriority v)
    {
        if (v == AlertPriority.Info)     return "info";
        if (v == AlertPriority.Medium)   return "media";
        if (v == AlertPriority.High)     return "alta";
        if (v == AlertPriority.Critical) return "critica";
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unmapped AlertPriority value.");
    }

    private static AlertPriority PriorityFromDb(string v)
    {
        if (v == "info")   return AlertPriority.Info;
        if (v == "media")  return AlertPriority.Medium;
        if (v == "alta")   return AlertPriority.High;
        if (v == "critica") return AlertPriority.Critical;
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unknown AlertPriority DB value.");
    }

    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alert");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.PatientId).HasColumnName("patient_id");
        builder.Property(a => a.DoctorId).HasColumnName("doctor_id");
        builder.Property(a => a.AiAnalysisId).HasColumnName("ai_analysis_id");
        builder.Property(a => a.Type).HasColumnName("type");
        builder.Property(a => a.Priority).HasColumnName("priority")
            .HasConversion(v => PriorityToDb(v), v => PriorityFromDb(v));
        builder.Property(a => a.Description).HasColumnName("description");
        builder.Property(a => a.Seen).HasColumnName("seen");
        builder.Property(a => a.Resolved).HasColumnName("resolved");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Ignore(a => a.DomainEvents);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Enums;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
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
            .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<AlertPriority>(v, true));
        builder.Property(a => a.Description).HasColumnName("description");
        builder.Property(a => a.Seen).HasColumnName("seen");
        builder.Property(a => a.Resolved).HasColumnName("resolved");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Ignore(a => a.DomainEvents);
    }
}

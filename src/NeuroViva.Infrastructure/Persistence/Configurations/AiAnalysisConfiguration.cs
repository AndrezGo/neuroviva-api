using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Enums;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class AiAnalysisConfiguration : IEntityTypeConfiguration<AiAnalysis>
{
    public void Configure(EntityTypeBuilder<AiAnalysis> builder)
    {
        builder.ToTable("ai_analysis");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.PatientId).HasColumnName("patient_id");
        builder.Property(a => a.Type).HasColumnName("type")
            .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<AnalysisType>(v, true));
        builder.Property(a => a.Summary).HasColumnName("summary");
        builder.Property(a => a.InputData).HasColumnName("input_data").HasColumnType("jsonb");
        builder.Property(a => a.Suggestions).HasColumnName("suggestions").HasColumnType("jsonb");
        builder.Property(a => a.OverallStatus).HasColumnName("overall_status")
            .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<OverallStatus>(v, true));
        builder.Property(a => a.GeneratedAt).HasColumnName("generated_at");
        builder.Ignore(a => a.DomainEvents);
    }
}

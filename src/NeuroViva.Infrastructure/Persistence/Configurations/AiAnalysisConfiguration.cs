using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Enums;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class AiAnalysisConfiguration : IEntityTypeConfiguration<AiAnalysis>
{
    // ia_analisis_tipo_check: diario, semanal, evento, solicitud
    private static string AnalysisTypeToDb(AnalysisType v)
    {
        if (v == AnalysisType.Daily)   return "diario";
        if (v == AnalysisType.Weekly)  return "semanal";
        if (v == AnalysisType.Event)   return "evento";
        if (v == AnalysisType.Request) return "solicitud";
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unmapped AnalysisType value.");
    }

    private static AnalysisType AnalysisTypeFromDb(string v)
    {
        if (v == "diario")    return AnalysisType.Daily;
        if (v == "semanal")   return AnalysisType.Weekly;
        if (v == "evento")    return AnalysisType.Event;
        if (v == "solicitud") return AnalysisType.Request;
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unknown AnalysisType DB value.");
    }

    // ia_analisis_estado_general_check: estable, atencion, alto, critico
    private static string OverallStatusToDb(OverallStatus v)
    {
        if (v == OverallStatus.Stable)    return "estable";
        if (v == OverallStatus.Attention) return "atencion";
        if (v == OverallStatus.High)      return "alto";
        if (v == OverallStatus.Critical)  return "critico";
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unmapped OverallStatus value.");
    }

    private static OverallStatus OverallStatusFromDb(string v)
    {
        if (v == "estable")   return OverallStatus.Stable;
        if (v == "atencion")  return OverallStatus.Attention;
        if (v == "alto")      return OverallStatus.High;
        if (v == "critico")   return OverallStatus.Critical;
        throw new ArgumentOutOfRangeException(nameof(v), v, "Unknown OverallStatus DB value.");
    }

    public void Configure(EntityTypeBuilder<AiAnalysis> builder)
    {
        builder.ToTable("ai_analysis");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.PatientId).HasColumnName("patient_id");
        builder.Property(a => a.Type).HasColumnName("type")
            .HasConversion(v => AnalysisTypeToDb(v), v => AnalysisTypeFromDb(v));
        builder.Property(a => a.Summary).HasColumnName("summary");
        builder.Property(a => a.InputData).HasColumnName("input_data").HasColumnType("jsonb");
        builder.Property(a => a.Suggestions).HasColumnName("suggestions").HasColumnType("jsonb");
        builder.Property(a => a.OverallStatus).HasColumnName("overall_status")
            .HasConversion(v => OverallStatusToDb(v), v => OverallStatusFromDb(v));
        builder.Property(a => a.GeneratedAt).HasColumnName("generated_at");
        builder.Ignore(a => a.DomainEvents);
    }
}

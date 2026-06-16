using NeuroViva.Domain.Ai.Enums;
using NeuroViva.Domain.Common;
using System.Text.Json;

namespace NeuroViva.Domain.Ai;

public sealed class AiAnalysis : AggregateRoot<Guid>
{
    public Guid PatientId { get; private set; }
    public AnalysisType Type { get; private set; }
    public string Summary { get; private set; } = default!;
    public JsonDocument InputData { get; private set; } = default!;
    public JsonDocument Suggestions { get; private set; } = default!;
    public OverallStatus OverallStatus { get; private set; }
    public DateTime GeneratedAt { get; private set; }

    private AiAnalysis() { }

    public static AiAnalysis Create(
        Guid patientId, AnalysisType type, string summary,
        OverallStatus status, JsonDocument? inputData = null, JsonDocument? suggestions = null) => new()
    {
        Id = Guid.NewGuid(),
        PatientId = patientId,
        Type = type,
        Summary = summary,
        OverallStatus = status,
        InputData = inputData ?? JsonDocument.Parse("{}"),
        Suggestions = suggestions ?? JsonDocument.Parse("[]"),
        GeneratedAt = DateTime.UtcNow
    };
}

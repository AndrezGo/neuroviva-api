using NeuroViva.Domain.Ai.Enums;

namespace NeuroViva.Application.Common.Abstractions;

public interface IAiAnalysisService
{
    Task<AiAnalysisResult> AnalyzePatientAsync(
        Guid patientId,
        AnalysisType type,
        object inputData,
        CancellationToken ct = default);
}

public sealed record AiAnalysisResult(
    string Summary,
    OverallStatus Status,
    IReadOnlyList<string> Suggestions);

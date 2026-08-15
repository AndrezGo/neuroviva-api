namespace NeuroViva.Application.MedicalRecords.Queries;

public sealed record HistoryEventTextDto(
    Guid Id,
    string Type,
    string Title,
    string? Description,
    DateTime EventDate,
    string? Status);

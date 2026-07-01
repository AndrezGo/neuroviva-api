namespace NeuroViva.Application.Caregivers.Queries.GetClinicalHistory;

public sealed record HistoryEventDto(
    Guid Id,
    string Type,
    string Title,
    string? Description,
    string EventDate,
    string? Status);

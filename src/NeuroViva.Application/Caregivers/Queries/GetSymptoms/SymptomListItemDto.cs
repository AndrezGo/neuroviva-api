namespace NeuroViva.Application.Caregivers.Queries.GetSymptoms;

public sealed record SymptomListItemDto(
    Guid Id,
    string Type,
    int Intensity,
    string? Description,
    string LoggedAt);

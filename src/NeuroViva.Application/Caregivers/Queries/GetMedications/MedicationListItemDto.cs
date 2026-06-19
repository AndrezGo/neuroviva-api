namespace NeuroViva.Application.Caregivers.Queries.GetMedications;

public sealed record MedicationListItemDto(
    Guid Id,
    string Name,
    string Dose,
    string Frequency,
    bool Active,
    // ISO 8601 date string (yyyy-MM-dd)
    string StartDate,
    // ISO 8601 date string (yyyy-MM-dd), or null if no end date
    string? EndDate,
    // ISO 8601 UTC datetime string
    string CreatedAt
);

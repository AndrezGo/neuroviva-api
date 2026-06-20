namespace NeuroViva.Application.Caregivers.Queries.GetMedicationLogs;

public sealed record MedicationLogItemDto(
    Guid Id,
    bool Taken,
    string LoggedAt,   // ISO 8601 UTC ("o")
    string? Notes,
    Guid LoggedBy
);

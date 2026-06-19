namespace NeuroViva.Application.Caregivers.Queries.GetToday;

public sealed record CaregiverTodayDto(
    IReadOnlyList<TodayMedicationDto> Medications,
    IReadOnlyList<TodayAppointmentDto> Appointments
);

public sealed record TodayMedicationDto(
    Guid Id,
    string Name,
    string Dose,
    // Free-text frequency string — frontend displays it as-is.
    string ScheduledTime,
    // "taken" or "pending". "skipped" is not supported in v1 (no skip semantics in schema).
    string Status,
    // Always false in v1: no structured schedule exists to determine "is now".
    bool IsNow
);

public sealed record TodayAppointmentDto(
    Guid Id,
    string Title,
    string Type,
    // ISO 8601 UTC string
    string ScheduledAt
);

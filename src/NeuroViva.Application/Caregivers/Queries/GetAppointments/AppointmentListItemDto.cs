namespace NeuroViva.Application.Caregivers.Queries.GetAppointments;

public sealed record AppointmentListItemDto(
    Guid Id,
    // First line of the notes field; falls back to capitalized type when notes is null/empty
    string Title,
    // Lowercase enum string: "consultation", "exam", "procedure", "teleconsultation"
    string Type,
    // ISO 8601 UTC datetime string
    string ScheduledAt,
    // Lowercase enum string: "scheduled", "confirmed", "completed", "cancelled"
    string Status
);

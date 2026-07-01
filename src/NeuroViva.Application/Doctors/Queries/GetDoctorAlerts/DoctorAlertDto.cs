namespace NeuroViva.Application.Doctors.Queries.GetDoctorAlerts;

public sealed record DoctorAlertDto(
    Guid Id,
    Guid PatientId,
    string PatientName,
    string Type,
    string Priority,    // "info" | "media" | "alta" | "critica"
    string Description,
    bool Seen,
    bool Resolved,
    DateTime CreatedAt
);

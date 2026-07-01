namespace NeuroViva.Application.Doctors.Queries.GetDoctorPatients;

public sealed record DoctorPatientDto(
    Guid PatientId,
    string Name,
    IReadOnlyList<string> Conditions, // disease names; empty if none assigned
    string? ConditionStage,      // always null in v1
    int Age,                     // 0 if DateOfBirth is null
    string? HighestAlertPriority,// null | "info" | "media" | "alta" | "critica"
    DateTime? LastActivityAt     // max(createdAt) across symptom_log, appointment, medication_log
);

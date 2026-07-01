namespace NeuroViva.Application.Doctors.Queries.GetDoctorPatients;

public sealed record DoctorPatientDto(
    Guid PatientId,
    string Name,
    string? Condition,           // disease.Name, null if no DiseaseId
    string? ConditionStage,      // always null in v1
    int Age,                     // 0 if DateOfBirth is null
    string? HighestAlertPriority,// null | "info" | "media" | "alta" | "critica"
    DateTime? LastActivityAt     // max(createdAt) across symptom_log, appointment, medication_log
);

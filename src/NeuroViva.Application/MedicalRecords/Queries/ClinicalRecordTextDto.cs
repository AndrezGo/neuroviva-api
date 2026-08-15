namespace NeuroViva.Application.MedicalRecords.Queries;

public sealed record ClinicalRecordTextDto(
    Guid Id,
    string EventType,
    string Description,
    DateTime EventDate);

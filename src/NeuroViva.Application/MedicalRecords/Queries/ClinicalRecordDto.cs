namespace NeuroViva.Application.MedicalRecords.Queries;

public sealed record ClinicalRecordDto(
    Guid Id,
    string EventType,
    string Description,
    string EventDate,
    IReadOnlyList<ClinicalRecordAttachmentDto> Attachments,
    DateTime CreatedAt);

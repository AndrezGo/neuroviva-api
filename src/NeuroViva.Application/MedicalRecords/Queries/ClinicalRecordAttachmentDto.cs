namespace NeuroViva.Application.MedicalRecords.Queries;

public sealed record ClinicalRecordAttachmentDto(
    Guid Id,
    string FileName,
    string ContentType,
    string? SignedUrl);

namespace NeuroViva.Application.MedicalRecords.Queries;

public sealed record ClinicalRecordAttachmentTextDto(
    string FileName,
    string ContentType,
    string? ExtractedText);

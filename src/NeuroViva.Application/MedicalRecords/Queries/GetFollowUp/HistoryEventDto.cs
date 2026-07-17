namespace NeuroViva.Application.MedicalRecords.Queries.GetFollowUp;

public sealed record HistoryEventDto(
    Guid Id,
    string Type,
    string Title,
    string? Description,
    string EventDate,
    string? Status,
    string? AttachmentUrl,
    string? AttachmentFileName);

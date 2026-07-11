namespace NeuroViva.Application.Patients.Queries.GetPatientFeed;

public sealed record ResourceDto(
    Guid Id,
    string Title,
    string Type,
    string? Url,
    string? Description,
    DateTime CreatedAt,
    string? EmbedUrl,
    Guid? ChannelId,
    string? ChannelName);

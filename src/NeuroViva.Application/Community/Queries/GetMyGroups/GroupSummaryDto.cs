namespace NeuroViva.Application.Community.Queries.GetMyGroups;

public sealed record GroupSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? AvatarUrl,
    Guid? DiseaseId,
    DateTime JoinedAt);

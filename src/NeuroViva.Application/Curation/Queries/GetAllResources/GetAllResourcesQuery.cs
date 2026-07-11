using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Curation.Queries.GetAllResources;

public sealed record GetAllResourcesQuery : IRequest<Result<IReadOnlyList<ResourceListItemDto>>>;

public sealed record ResourceListItemDto(
    Guid Id,
    Guid AuthorId,
    Guid? DiseaseId,
    string Title,
    string Type,
    string? Url,
    string? Description,
    DateTime CreatedAt,
    Guid? ChannelId,
    string? ChannelName,
    string ApprovalStatus);

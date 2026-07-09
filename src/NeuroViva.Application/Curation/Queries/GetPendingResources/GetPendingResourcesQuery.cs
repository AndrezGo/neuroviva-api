using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Curation.Queries.GetPendingResources;

public sealed record GetPendingResourcesQuery : IRequest<Result<IReadOnlyList<PendingResourceDto>>>;

public sealed record PendingResourceDto(
    Guid Id,
    Guid AuthorId,
    Guid? DiseaseId,
    string Title,
    string Type,
    string? Url,
    string? Description,
    DateTime CreatedAt);

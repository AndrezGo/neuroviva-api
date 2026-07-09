using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Content.Repositories;

namespace NeuroViva.Application.Curation.Queries.GetPendingResources;

public sealed class GetPendingResourcesQueryHandler
    : IRequestHandler<GetPendingResourcesQuery, Result<IReadOnlyList<PendingResourceDto>>>
{
    private readonly IResourceRepository _resourceRepo;

    public GetPendingResourcesQueryHandler(IResourceRepository resourceRepo)
        => _resourceRepo = resourceRepo;

    public async Task<Result<IReadOnlyList<PendingResourceDto>>> Handle(
        GetPendingResourcesQuery request,
        CancellationToken cancellationToken)
    {
        var resources = await _resourceRepo.ListPendingAsync(cancellationToken);

        var dtos = resources.Select(r => new PendingResourceDto(
            Id: r.Id,
            AuthorId: r.AuthorId,
            DiseaseId: r.DiseaseId,
            Title: r.Title,
            Type: r.Type.ToString(),
            Url: r.Url,
            Description: r.Description,
            CreatedAt: r.CreatedAt
        )).ToList();

        return Result<IReadOnlyList<PendingResourceDto>>.Success(dtos);
    }
}

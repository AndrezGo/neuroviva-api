using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Catalog.Repositories;

namespace NeuroViva.Application.Curation.Queries.GetActiveDiseases;

public sealed class GetActiveDiseasesQueryHandler
    : IRequestHandler<GetActiveDiseasesQuery, Result<IReadOnlyList<DiseaseDto>>>
{
    private readonly IDiseaseRepository _diseaseRepo;

    public GetActiveDiseasesQueryHandler(IDiseaseRepository diseaseRepo)
        => _diseaseRepo = diseaseRepo;

    public async Task<Result<IReadOnlyList<DiseaseDto>>> Handle(
        GetActiveDiseasesQuery request,
        CancellationToken cancellationToken)
    {
        var diseases = await _diseaseRepo.ListActiveAsync(cancellationToken);

        var dtos = diseases.Select(d => new DiseaseDto(
            Id: d.Id,
            Name: d.Name,
            Slug: d.Slug,
            Category: d.Category
        )).ToList();

        return Result<IReadOnlyList<DiseaseDto>>.Success(dtos);
    }
}

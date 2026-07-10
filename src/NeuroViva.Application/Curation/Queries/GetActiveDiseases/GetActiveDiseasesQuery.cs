using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Curation.Queries.GetActiveDiseases;

public sealed record GetActiveDiseasesQuery : IRequest<Result<IReadOnlyList<DiseaseDto>>>;

public sealed record DiseaseDto(
    Guid Id,
    string Name,
    string Slug,
    string? Category);

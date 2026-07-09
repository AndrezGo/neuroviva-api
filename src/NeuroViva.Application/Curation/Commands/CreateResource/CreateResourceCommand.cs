using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Content.Enums;

namespace NeuroViva.Application.Curation.Commands.CreateResource;

public sealed record CreateResourceCommand(
    string Title,
    ResourceType Type,
    string? Url,
    string? Description,
    Guid? DiseaseId) : IRequest<Result<CreateResourceResult>>;

public sealed record CreateResourceResult(Guid ResourceId);

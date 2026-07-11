using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Content.Enums;

namespace NeuroViva.Application.Curation.Commands.UpdateResource;

public sealed record UpdateResourceCommand(
    Guid Id,
    string Title,
    ResourceType Type,
    string? Url,
    string? Description,
    Guid? DiseaseId,
    Guid? ChannelId) : IRequest<Result>;

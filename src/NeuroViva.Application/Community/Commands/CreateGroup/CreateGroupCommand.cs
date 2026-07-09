using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Community.Commands.CreateGroup;

public sealed record CreateGroupCommand(
    string Name,
    string Slug,
    string? Description,
    string? AvatarUrl,
    string Visibility = "public",
    Guid? DiseaseId = null) : IRequest<Result<CreateGroupResult>>;

public sealed record CreateGroupResult(Guid GroupId);

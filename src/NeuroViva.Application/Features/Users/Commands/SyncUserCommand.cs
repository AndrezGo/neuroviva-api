using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Features.Users.Dtos;

namespace NeuroViva.Application.Features.Users.Commands;

public sealed record SyncUserCommand(
    Guid AuthUserId,
    string Email,
    string? Name,
    Guid? TenantId  // optional — auto-creates personal tenant when null
) : IRequest<Result<CurrentUserDto>>;

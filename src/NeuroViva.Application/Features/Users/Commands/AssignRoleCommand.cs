using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Features.Users.Dtos;

namespace NeuroViva.Application.Features.Users.Commands;

public sealed record AssignRoleCommand(
    Guid UserId,
    string RoleName
) : IRequest<Result<CurrentUserDto>>;

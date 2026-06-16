using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Features.Users.Dtos;

namespace NeuroViva.Application.Features.Users.Queries;

public sealed record GetCurrentUserQuery : IRequest<Result<CurrentUserDto>>;

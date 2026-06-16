using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Features.Users.Dtos;

namespace NeuroViva.Application.Features.Users.Queries;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserReadRepository _repo;

    public GetCurrentUserQueryHandler(ICurrentUserService currentUser, IUserReadRepository repo)
    {
        _currentUser = currentUser;
        _repo = repo;
    }

    public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not found in system. Call /users/sync first.");

        var dto = await _repo.GetCurrentUserDtoAsync(_currentUser.UserId.Value, cancellationToken);
        if (dto is null)
            return Error.NotFound("user.not_found", "Current user not found.");

        return dto;
    }
}

using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Features.Users.Dtos;
using NeuroViva.Application.Features.Users.Queries;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Users;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Features.Users.Commands;

public sealed class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result<CurrentUserDto>>
{
    private readonly IRoleRepository _roleRepo;
    private readonly IUserRoleRepository _userRoleRepo;
    private readonly IUnitOfWork _uow;
    private readonly IUserReadRepository _readRepo;

    public AssignRoleCommandHandler(
        IRoleRepository roleRepo,
        IUserRoleRepository userRoleRepo,
        IUnitOfWork uow,
        IUserReadRepository readRepo)
    {
        _roleRepo = roleRepo;
        _userRoleRepo = userRoleRepo;
        _uow = uow;
        _readRepo = readRepo;
    }

    public async Task<Result<CurrentUserDto>> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepo.GetByNameAsync(request.RoleName, cancellationToken);
        if (role is null)
            return Error.NotFound("role.not_found", $"Role '{request.RoleName}' not found.");

        var alreadyAssigned = await _userRoleRepo.ExistsAsync(request.UserId, role.Id, cancellationToken);
        if (!alreadyAssigned)
        {
            var userRole = UserRole.Assign(request.UserId, role.Id);
            await _userRoleRepo.AddAsync(userRole, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        var dto = await _readRepo.GetCurrentUserDtoAsync(request.UserId, cancellationToken);
        return dto!;
    }
}

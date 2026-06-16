using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Features.Users.Dtos;
using NeuroViva.Application.Features.Users.Queries;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Users;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Features.Users.Commands;

public sealed class SyncUserCommandHandler : IRequestHandler<SyncUserCommand, Result<CurrentUserDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly IUserReadRepository _readRepo;

    public SyncUserCommandHandler(
        IUserRepository userRepo,
        IUnitOfWork uow,
        IUserReadRepository readRepo)
    {
        _userRepo = userRepo;
        _uow = uow;
        _readRepo = readRepo;
    }

    public async Task<Result<CurrentUserDto>> Handle(SyncUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepo.GetByAuthUserIdAsync(request.AuthUserId, cancellationToken);
        if (existing is not null)
        {
            var existingDto = await _readRepo.GetCurrentUserDtoAsync(existing.Id, cancellationToken);
            return existingDto!;
        }

        var user = User.Create(
            tenantId: request.TenantId,
            name: request.Name ?? request.Email,
            email: request.Email,
            authUserId: request.AuthUserId);

        await _userRepo.AddAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var dto = await _readRepo.GetCurrentUserDtoAsync(user.Id, cancellationToken);
        return dto!;
    }
}

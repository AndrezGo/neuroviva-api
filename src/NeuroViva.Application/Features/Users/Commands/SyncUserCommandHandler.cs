// NOTE: trial subscription creation is delegated to the Supabase AFTER INSERT trigger on tenant.
// If this code is ever ported to a database without that trigger, reintroduce subscription creation here
// with an idempotency check: await _subscriptionRepo.GetByTenantAsync(tenant.Id) before AddAsync.

using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Features.Users.Dtos;
using NeuroViva.Application.Features.Users.Queries;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Tenancy;
using NeuroViva.Domain.Tenancy.Repositories;
using NeuroViva.Domain.Users;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Features.Users.Commands;

public sealed class SyncUserCommandHandler : IRequestHandler<SyncUserCommand, Result<CurrentUserDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly ITenantRepository _tenantRepo;
    private readonly IUnitOfWork _uow;
    private readonly IUserReadRepository _readRepo;

    public SyncUserCommandHandler(
        IUserRepository userRepo,
        ITenantRepository tenantRepo,
        IUnitOfWork uow,
        IUserReadRepository readRepo)
    {
        _userRepo = userRepo;
        _tenantRepo = tenantRepo;
        _uow = uow;
        _readRepo = readRepo;
    }

    public async Task<Result<CurrentUserDto>> Handle(SyncUserCommand request, CancellationToken cancellationToken)
    {
        // If the user already exists, return the current snapshot
        var existing = await _userRepo.GetByAuthUserIdAsync(request.AuthUserId, cancellationToken);
        if (existing is not null)
        {
            if (request.Name is not null && existing.Name == existing.Email)
            {
                existing.UpdateName(request.Name);
                _userRepo.Update(existing);
                await _uow.SaveChangesAsync(cancellationToken);
            }

            var existingDto = await _readRepo.GetCurrentUserDtoAsync(existing.Id, cancellationToken);
            return existingDto!;
        }

        await using var tx = await _uow.BeginTransactionAsync(cancellationToken);

        // Resolve or auto-create a tenant
        Guid tenantId;
        if (request.TenantId.HasValue)
        {
            tenantId = request.TenantId.Value;
        }
        else
        {
            // Auto-create a personal tenant for the new user
            var tenant = Tenant.Create(request.Name ?? request.Email);
            await _tenantRepo.AddAsync(tenant, cancellationToken);
            // Persist the tenant; the Supabase AFTER INSERT trigger on tenant creates the trial subscription.
            await _uow.SaveChangesAsync(cancellationToken);

            tenantId = tenant.Id;
        }

        var user = User.Create(
            tenantId: tenantId,
            name: request.Name ?? request.Email,
            email: request.Email,
            authUserId: request.AuthUserId);

        await _userRepo.AddAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);

        var dto = await _readRepo.GetCurrentUserDtoAsync(user.Id, cancellationToken);
        return dto!;
    }
}

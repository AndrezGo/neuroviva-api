using Microsoft.EntityFrameworkCore;
using NeuroViva.Application.Features.Users.Dtos;
using NeuroViva.Application.Features.Users.Queries;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.ReadRepositories;

public sealed class UserReadRepository : IUserReadRepository
{
    private readonly NeuroVivaDbContext _db;

    public UserReadRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<CurrentUserDto?> GetCurrentUserDtoAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return null;

        var roles = await _db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .ToListAsync(ct);

        return new CurrentUserDto(
            Id: user.Id,
            TenantId: user.TenantId,
            Name: user.Name,
            Email: user.Email,
            AvatarUrl: user.AvatarUrl,
            IsActive: user.IsActive,
            Roles: roles,
            CreatedAt: user.CreatedAt);
    }

    public async Task<UserClaimsData?> GetClaimsByAuthUserIdAsync(Guid authUserId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.AuthUserId == authUserId, ct);

        if (user is null) return null;

        var roles = await _db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == user.Id)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .ToListAsync(ct);

        return new UserClaimsData(user.Id, user.TenantId, roles);
    }
}

using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Users;
using NeuroViva.Domain.Users.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly NeuroVivaDbContext _db;

    public UserRoleRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken ct = default)
        => await _db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);

    public async Task AddAsync(UserRole userRole, CancellationToken ct = default)
        => await _db.UserRoles.AddAsync(userRole, ct);
}

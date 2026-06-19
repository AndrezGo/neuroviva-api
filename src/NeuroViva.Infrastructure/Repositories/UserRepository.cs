using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Users;
using NeuroViva.Domain.Users.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly NeuroVivaDbContext _db;

    public UserRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByAuthUserIdAsync(Guid authUserId, CancellationToken ct = default)
        => await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.AuthUserId == authUserId, ct);

    public async Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default)
        => await _db.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email, ct);

    public async Task<IReadOnlyList<User>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.Users.Where(u => u.TenantId == tenantId).ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _db.Users.AddAsync(user, ct);

    public void Update(User user) => _db.Users.Update(user);
}

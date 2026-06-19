using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Users;
using NeuroViva.Domain.Users.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly NeuroVivaDbContext _db;

    public RoleRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        => await _db.Roles.FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);
}

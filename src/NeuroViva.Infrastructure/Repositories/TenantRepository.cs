using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Tenancy;
using NeuroViva.Domain.Tenancy.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly NeuroVivaDbContext _db;

    public TenantRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
        => await _db.Tenants.AddAsync(tenant, ct);
}

namespace NeuroViva.Domain.Tenancy.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
}

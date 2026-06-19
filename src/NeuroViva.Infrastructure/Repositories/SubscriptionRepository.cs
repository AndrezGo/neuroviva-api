using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Billing;
using NeuroViva.Domain.Billing.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly NeuroVivaDbContext _db;

    public SubscriptionRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Subscription?> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.Subscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

    public async Task AddAsync(Subscription subscription, CancellationToken ct = default)
        => await _db.Subscriptions.AddAsync(subscription, ct);

    public void Update(Subscription subscription)
        => _db.Subscriptions.Update(subscription);
}

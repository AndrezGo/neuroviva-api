using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Billing;
using NeuroViva.Domain.Billing.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly NeuroVivaDbContext _db;

    public SubscriptionPlanRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<SubscriptionPlan?> GetFirstActiveAsync(CancellationToken ct = default)
        => await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Active, ct);
}

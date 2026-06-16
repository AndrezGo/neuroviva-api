namespace NeuroViva.Domain.Billing.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Subscription subscription, CancellationToken ct = default);
    void Update(Subscription subscription);
}

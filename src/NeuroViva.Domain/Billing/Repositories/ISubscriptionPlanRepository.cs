namespace NeuroViva.Domain.Billing.Repositories;

public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> GetFirstActiveAsync(CancellationToken ct = default);
}

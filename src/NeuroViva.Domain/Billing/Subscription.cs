using NeuroViva.Domain.Billing.Enums;
using NeuroViva.Domain.Common;
using NeuroViva.Domain.Exceptions;

namespace NeuroViva.Domain.Billing;

public sealed class Subscription : AggregateRoot<Guid>, ITenantOwned
{
    public Guid TenantId { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime TrialStart { get; private set; }
    public DateTime TrialEnd { get; private set; }
    public DateTime? PeriodStart { get; private set; }
    public DateTime? PeriodEnd { get; private set; }
    public bool CardRegistered { get; private set; }
    public DateTime? CardRegisteredAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Subscription() { }

    public static Subscription StartTrial(Guid tenantId, Guid planId, int trialDays = 7)
    {
        var now = DateTime.UtcNow;
        return new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanId = planId,
            Status = SubscriptionStatus.Trial,
            TrialStart = now,
            TrialEnd = now.AddDays(trialDays),
            CreatedAt = now
        };
    }

    public void RegisterPaymentMethod()
    {
        CardRegistered = true;
        CardRegisteredAt = DateTime.UtcNow;
    }

    public void Activate(DateTime periodStart, DateTime periodEnd)
    {
        if (!CardRegistered)
            throw new BusinessRuleViolationException(
                "subscription.payment_method_required",
                "A payment method must be registered before activating a subscription.");

        Status = SubscriptionStatus.Active;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
    }

    public void Expire() => Status = SubscriptionStatus.Expired;
    public void Cancel() => Status = SubscriptionStatus.Cancelled;
    public void Pause() => Status = SubscriptionStatus.Paused;
}

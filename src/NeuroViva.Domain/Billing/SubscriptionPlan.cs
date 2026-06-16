using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Billing;

public sealed class SubscriptionPlan : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public decimal MonthlyPrice { get; private set; }
    public int TrialDays { get; private set; }
    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private SubscriptionPlan() { }
}

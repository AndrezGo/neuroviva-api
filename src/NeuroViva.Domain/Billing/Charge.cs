using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Billing;

public sealed class Charge : Entity<Guid>
{
    public Guid SubscriptionId { get; private set; }
    public Guid? PaymentMethodId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "COP";
    public string Status { get; private set; } = default!;
    public string? GatewayReference { get; private set; }
    public DateTime ChargedAt { get; private set; }
    public DateTime? NextChargeAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private Charge() { }
    public static Charge Create(Guid subscriptionId, decimal amount, Guid? paymentMethodId = null) =>
        new() { Id = Guid.NewGuid(), SubscriptionId = subscriptionId, PaymentMethodId = paymentMethodId, Amount = amount, Currency = "COP", Status = "pending", ChargedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
}

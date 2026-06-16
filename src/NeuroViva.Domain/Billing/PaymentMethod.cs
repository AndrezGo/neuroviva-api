using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Billing;

public sealed class PaymentMethod : Entity<Guid>, ITenantOwned
{
    public Guid TenantId { get; private set; }
    public string Type { get; private set; } = default!;
    public string? Last4 { get; private set; }
    public string? Brand { get; private set; }
    public string GatewayToken { get; private set; } = default!;
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private PaymentMethod() { }
    public static PaymentMethod Create(Guid tenantId, string type, string gatewayToken, string? last4 = null, string? brand = null) =>
        new() { Id = Guid.NewGuid(), TenantId = tenantId, Type = type, GatewayToken = gatewayToken, Last4 = last4, Brand = brand, IsDefault = false, CreatedAt = DateTime.UtcNow };
    public void SetDefault() => IsDefault = true;
}

using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Marketplace;

public sealed class MarketplaceApproval : Entity<Guid>
{
    public Guid StoreId { get; private set; }
    public string Stage { get; private set; } = default!;
    public string Status { get; private set; } = default!;
    public Guid? ReviewedBy { get; private set; }
    public string? Comment { get; private set; }
    public DateTime ReviewedAt { get; private set; }
    private MarketplaceApproval() { }
    public static MarketplaceApproval Create(Guid storeId, string stage, string status) => new()
    {
        Id = Guid.NewGuid(), StoreId = storeId, Stage = stage, Status = status, ReviewedAt = DateTime.UtcNow
    };
}

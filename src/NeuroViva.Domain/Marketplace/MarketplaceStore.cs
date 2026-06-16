using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Marketplace;

public sealed class MarketplaceStore : AggregateRoot<Guid>
{
    public Guid OwnerId { get; private set; }
    public Guid? DiseaseId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string StoreUrl { get; private set; } = default!;
    public string? LogoUrl { get; private set; }
    public string? Category { get; private set; }
    public string ApprovalStatus { get; private set; } = "pending";
    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private MarketplaceStore() { }
    public static MarketplaceStore Create(Guid ownerId, string name, string storeUrl, Guid? diseaseId = null) => new()
    {
        Id = Guid.NewGuid(), OwnerId = ownerId, Name = name, StoreUrl = storeUrl, DiseaseId = diseaseId, ApprovalStatus = "pending", Active = false, CreatedAt = DateTime.UtcNow
    };
    public void Approve() { ApprovalStatus = "approved"; Active = true; }
    public void Reject() { ApprovalStatus = "rejected"; Active = false; }
}

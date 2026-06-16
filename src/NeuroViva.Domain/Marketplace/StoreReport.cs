using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Marketplace;

public sealed class StoreReport : Entity<Guid>
{
    public Guid StoreId { get; private set; }
    public Guid ReportedBy { get; private set; }
    public string Reason { get; private set; } = default!;
    public string? Description { get; private set; }
    public string Status { get; private set; } = "pending";
    public DateTime CreatedAt { get; private set; }
    private StoreReport() { }
    public static StoreReport Create(Guid storeId, Guid reportedBy, string reason, string? description = null) => new()
    {
        Id = Guid.NewGuid(), StoreId = storeId, ReportedBy = reportedBy, Reason = reason, Description = description, Status = "pending", CreatedAt = DateTime.UtcNow
    };
}

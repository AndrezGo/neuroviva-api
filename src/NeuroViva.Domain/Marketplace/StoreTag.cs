using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Marketplace;

public sealed class StoreTag : Entity<Guid>
{
    public Guid StoreId { get; private set; }
    public string Tag { get; private set; } = default!;
    private StoreTag() { }
    public static StoreTag Create(Guid storeId, string tag) => new() { Id = Guid.NewGuid(), StoreId = storeId, Tag = tag };
}

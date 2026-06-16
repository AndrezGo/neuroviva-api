using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Catalog;

public sealed class Disease : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? Category { get; private set; }
    public bool IsActive { get; private set; }

    private Disease() { }
}

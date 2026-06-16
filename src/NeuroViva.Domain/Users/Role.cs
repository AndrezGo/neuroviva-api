using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Users;

public sealed class Role : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    private Role() { }
}

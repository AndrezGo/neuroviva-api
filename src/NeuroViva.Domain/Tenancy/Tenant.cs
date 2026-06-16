using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Tenancy;

public sealed class Tenant : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Domain { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Tenant() { }

    public static Tenant Create(string name, string? domain = null)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Domain = domain,
            CreatedAt = DateTime.UtcNow
        };
    }
}

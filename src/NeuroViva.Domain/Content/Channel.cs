using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Content;

public sealed class Channel : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Channel() { }

    public static Channel Create(string name, string? description = null, string? avatarUrl = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = name.Trim(),
        Description = description,
        AvatarUrl = avatarUrl,
        CreatedAt = DateTime.UtcNow
    };

    public void Update(string name, string? description, string? avatarUrl)
    {
        Name = name.Trim();
        Description = description;
        AvatarUrl = avatarUrl;
    }
}

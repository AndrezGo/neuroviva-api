using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Community;

public sealed class CommunityReaction : Entity<Guid>
{
    public Guid PostId { get; private set; }
    public Guid UserId { get; private set; }
    public string Type { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    private CommunityReaction() { }
    public static CommunityReaction Add(Guid postId, Guid userId, string type) => new()
    {
        Id = Guid.NewGuid(), PostId = postId, UserId = userId, Type = type, CreatedAt = DateTime.UtcNow
    };
}

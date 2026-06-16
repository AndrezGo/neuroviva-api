using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Community;

public sealed class GroupReaction : Entity<Guid>
{
    public Guid MessageId { get; private set; }
    public Guid UserId { get; private set; }
    public string Emoji { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    private GroupReaction() { }
    public static GroupReaction Add(Guid messageId, Guid userId, string emoji) => new()
    {
        Id = Guid.NewGuid(), MessageId = messageId, UserId = userId, Emoji = emoji, CreatedAt = DateTime.UtcNow
    };
}

using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Community;

public sealed class CommunityComment : Entity<Guid>
{
    public Guid PostId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Content { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    private CommunityComment() { }
    public static CommunityComment Create(Guid postId, Guid authorId, string content) => new()
    {
        Id = Guid.NewGuid(), PostId = postId, AuthorId = authorId, Content = content, CreatedAt = DateTime.UtcNow
    };
}

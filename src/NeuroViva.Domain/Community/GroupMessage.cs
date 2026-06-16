using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Community;

public sealed class GroupMessage : AggregateRoot<Guid>
{
    public Guid GroupId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Content { get; private set; } = default!;
    public string Type { get; private set; } = "text";
    public string? FileUrl { get; private set; }
    public Guid? ReplyTo { get; private set; }
    public bool Deleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private GroupMessage() { }
    public static GroupMessage Send(Guid groupId, Guid authorId, string content, string type = "text", string? fileUrl = null, Guid? replyTo = null) => new()
    {
        Id = Guid.NewGuid(), GroupId = groupId, AuthorId = authorId, Content = content, Type = type, FileUrl = fileUrl, ReplyTo = replyTo, Deleted = false, CreatedAt = DateTime.UtcNow
    };
    public void SoftDelete() => Deleted = true;
}

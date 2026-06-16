using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Community;

public sealed class GroupMember : Entity<Guid>
{
    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }
    public string Role { get; private set; } = "member";
    public bool Muted { get; private set; }
    public DateTime? MutedUntil { get; private set; }
    public string Status { get; private set; } = "active";
    public DateTime JoinedAt { get; private set; }
    private GroupMember() { }
    public static GroupMember Join(Guid groupId, Guid userId, string role = "member") => new()
    {
        Id = Guid.NewGuid(), GroupId = groupId, UserId = userId, Role = role, Muted = false, Status = "active", JoinedAt = DateTime.UtcNow
    };
    public void Leave() => Status = "left";
    public void Mute(DateTime until) { Muted = true; MutedUntil = until; }
}

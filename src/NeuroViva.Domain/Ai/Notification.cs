using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Ai;

public sealed class Notification : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid? AlertId { get; private set; }
    public string Channel { get; private set; } = default!;
    public string Status { get; private set; } = "pending";
    public DateTime? SentAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private Notification() { }
    public static Notification Create(Guid userId, string channel, Guid? alertId = null) => new()
    {
        Id = Guid.NewGuid(), UserId = userId, AlertId = alertId, Channel = channel, Status = "pending", CreatedAt = DateTime.UtcNow
    };
    public void MarkSent() { Status = "sent"; SentAt = DateTime.UtcNow; }
    public void MarkFailed() => Status = "failed";
}

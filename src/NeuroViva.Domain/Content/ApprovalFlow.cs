using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Content;

public sealed class ApprovalFlow : Entity<Guid>
{
    public Guid ResourceId { get; private set; }
    public string Stage { get; private set; } = default!;
    public string Status { get; private set; } = default!;
    public Guid? ReviewedBy { get; private set; }
    public string? Comment { get; private set; }
    public DateTime ReviewedAt { get; private set; }
    private ApprovalFlow() { }
    public static ApprovalFlow Create(Guid resourceId, string stage, string status) => new()
    {
        Id = Guid.NewGuid(), ResourceId = resourceId, Stage = stage, Status = status, ReviewedAt = DateTime.UtcNow
    };
}

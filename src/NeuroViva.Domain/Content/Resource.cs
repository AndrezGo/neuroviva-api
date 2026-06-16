using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Content;

public sealed class Resource : Entity<Guid>
{
    public Guid AuthorId { get; private set; }
    public Guid? DiseaseId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Type { get; private set; } = default!;
    public string? Url { get; private set; }
    public string? Description { get; private set; }
    public string ApprovalStatus { get; private set; } = "pending";
    public DateTime CreatedAt { get; private set; }
    private Resource() { }
    public static Resource Create(Guid authorId, string title, string type, Guid? diseaseId = null, string? url = null, string? description = null) => new()
    {
        Id = Guid.NewGuid(), AuthorId = authorId, Title = title, Type = type, DiseaseId = diseaseId, Url = url, Description = description, CreatedAt = DateTime.UtcNow
    };
    public void Approve() => ApprovalStatus = "approved";
    public void Reject() => ApprovalStatus = "rejected";
}

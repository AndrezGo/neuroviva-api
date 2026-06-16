using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Community;

public sealed class Group : AggregateRoot<Guid>
{
    public Guid CreatorId { get; private set; }
    public Guid? DiseaseId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string Visibility { get; private set; } = "public";
    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private Group() { }
    public static Group Create(Guid creatorId, string name, string slug, string visibility = "public", Guid? diseaseId = null) => new()
    {
        Id = Guid.NewGuid(), CreatorId = creatorId, Name = name, Slug = slug, Visibility = visibility, DiseaseId = diseaseId, Active = true, CreatedAt = DateTime.UtcNow
    };
}

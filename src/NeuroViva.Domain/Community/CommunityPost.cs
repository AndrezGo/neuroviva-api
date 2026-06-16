using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Community;

public sealed class CommunityPost : AggregateRoot<Guid>
{
    public Guid AuthorId { get; private set; }
    public Guid? PatientId { get; private set; }
    public Guid? DiseaseId { get; private set; }
    public string Content { get; private set; } = default!;
    public string Visibility { get; private set; } = "public";
    public DateTime CreatedAt { get; private set; }
    private CommunityPost() { }
    public static CommunityPost Create(Guid authorId, string content, string visibility = "public", Guid? patientId = null, Guid? diseaseId = null) => new()
    {
        Id = Guid.NewGuid(), AuthorId = authorId, Content = content, Visibility = visibility, PatientId = patientId, DiseaseId = diseaseId, CreatedAt = DateTime.UtcNow
    };
}

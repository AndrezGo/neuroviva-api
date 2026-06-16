using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Content;

public sealed class CaregiverCourse : Entity<Guid>
{
    public Guid? DiseaseId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Type { get; private set; } = default!;
    public string? ContentUrl { get; private set; }
    public int? DurationMin { get; private set; }
    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private CaregiverCourse() { }
}

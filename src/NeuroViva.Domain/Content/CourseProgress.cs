using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Content;

public sealed class CourseProgress : Entity<Guid>
{
    public Guid CourseId { get; private set; }
    public Guid CaregiverId { get; private set; }
    public int Percentage { get; private set; }
    public bool Completed { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    private CourseProgress() { }
    public static CourseProgress Start(Guid courseId, Guid caregiverId) => new()
    {
        Id = Guid.NewGuid(), CourseId = courseId, CaregiverId = caregiverId, Percentage = 0, Completed = false, LastActivityAt = DateTime.UtcNow
    };
    public void UpdateProgress(int percentage) { Percentage = Math.Clamp(percentage, 0, 100); Completed = Percentage == 100; LastActivityAt = DateTime.UtcNow; }
}

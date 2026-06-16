using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Content;

public sealed class ResourcePatient : Entity<Guid>
{
    public Guid ResourceId { get; private set; }
    public Guid PatientId { get; private set; }
    public bool Completed { get; private set; }
    public int Progress { get; private set; }
    public DateTime AssignedAt { get; private set; }
    private ResourcePatient() { }
    public static ResourcePatient Assign(Guid resourceId, Guid patientId) => new()
    {
        Id = Guid.NewGuid(), ResourceId = resourceId, PatientId = patientId, Completed = false, Progress = 0, AssignedAt = DateTime.UtcNow
    };
    public void UpdateProgress(int percentage) { Progress = Math.Clamp(percentage, 0, 100); if (Progress == 100) Completed = true; }
}

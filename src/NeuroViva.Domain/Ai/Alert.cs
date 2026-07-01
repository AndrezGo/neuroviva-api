using NeuroViva.Domain.Ai.Enums;
using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Ai;

public sealed class Alert : AggregateRoot<Guid>
{
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid? AiAnalysisId { get; private set; }
    public Guid? SourceReferenceId { get; private set; }
    public string Type { get; private set; } = default!;
    public AlertPriority Priority { get; private set; }
    public string Description { get; private set; } = default!;
    public bool Seen { get; private set; }
    public bool Resolved { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Alert() { }

    public static Alert Create(
        Guid patientId, Guid doctorId, string type,
        AlertPriority priority, string description,
        Guid? analysisId = null,
        Guid? sourceReferenceId = null) => new()
    {
        Id = Guid.NewGuid(),
        PatientId = patientId,
        DoctorId = doctorId,
        AiAnalysisId = analysisId,
        SourceReferenceId = sourceReferenceId,
        Type = type,
        Priority = priority,
        Description = description,
        Seen = false,
        Resolved = false,
        CreatedAt = DateTime.UtcNow
    };

    public void MarkSeen() => Seen = true;
    public void Resolve() => Resolved = true;
}

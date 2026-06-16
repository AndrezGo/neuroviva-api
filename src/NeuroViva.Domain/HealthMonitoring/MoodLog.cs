using NeuroViva.Domain.Common;
using NeuroViva.Domain.HealthMonitoring.ValueObjects;

namespace NeuroViva.Domain.HealthMonitoring;

public sealed class MoodLog : AggregateRoot<Guid>
{
    public Guid PatientId { get; private set; }
    public Guid LoggedBy { get; private set; }
    public int LevelValue { get; private set; }
    public string? Note { get; private set; }
    public DateTime LoggedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private MoodLog() { }

    public static MoodLog Register(
        Guid patientId,
        Guid loggedBy,
        int level,
        string? note = null,
        DateTime? loggedAt = null)
    {
        var levelVo = MoodLevel.Of(level);
        return new MoodLog
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            LoggedBy = loggedBy,
            LevelValue = levelVo.Value,
            Note = note,
            LoggedAt = loggedAt ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }
}

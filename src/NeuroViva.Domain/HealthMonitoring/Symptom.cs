using NeuroViva.Domain.Common;
using NeuroViva.Domain.HealthMonitoring.Events;
using NeuroViva.Domain.HealthMonitoring.ValueObjects;

namespace NeuroViva.Domain.HealthMonitoring;

public sealed class Symptom : AggregateRoot<Guid>
{
    public Guid PatientId { get; private set; }
    public Guid LoggedBy { get; private set; }
    public string Type { get; private set; } = default!;
    public int IntensityValue { get; private set; }
    public string? Description { get; private set; }
    public DateTime LoggedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    private Symptom() { }

    public static Symptom Register(
        Guid patientId,
        Guid loggedBy,
        string type,
        int intensity,
        string? description = null,
        DateTime? loggedAt = null)
    {
        var intensityVo = Intensity.Of(intensity);

        var symptom = new Symptom
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            LoggedBy = loggedBy,
            Type = type,
            IntensityValue = intensityVo.Value,
            Description = description,
            LoggedAt = loggedAt ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        if (intensityVo.IsHigh)
            symptom.RaiseEvent(new HighIntensitySymptomDomainEvent(
                symptom.Id, patientId, type, intensityVo.Value));

        return symptom;
    }

    public void Update(string type, int intensity, string? description)
    {
        var intensityVo = Intensity.Of(intensity);

        Type = type;
        IntensityValue = intensityVo.Value;
        Description = description;

        if (intensityVo.IsHigh)
            RaiseEvent(new HighIntensitySymptomDomainEvent(
                Id, PatientId, type, intensityVo.Value));
    }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
    }
}

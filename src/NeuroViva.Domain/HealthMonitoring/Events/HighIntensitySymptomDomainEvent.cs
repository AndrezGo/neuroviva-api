using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.HealthMonitoring.Events;

public sealed record HighIntensitySymptomDomainEvent(
    Guid SymptomId,
    Guid PatientId,
    string SymptomType,
    int Intensity
) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

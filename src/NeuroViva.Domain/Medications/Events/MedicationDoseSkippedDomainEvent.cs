using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Medications.Events;

public sealed record MedicationDoseSkippedDomainEvent(
    Guid MedicationLogId,
    Guid PatientId,
    Guid MedicationId,
    string MedicationName
) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

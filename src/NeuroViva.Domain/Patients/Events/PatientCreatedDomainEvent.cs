using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Patients.Events;

public sealed record PatientCreatedDomainEvent(Guid PatientId, Guid TenantId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

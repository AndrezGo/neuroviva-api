using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Appointments.Events;

public sealed record AppointmentCancelledDomainEvent(
    Guid AppointmentId,
    Guid PatientId,
    string AppointmentTitle,
    DateTime ScheduledAt
) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

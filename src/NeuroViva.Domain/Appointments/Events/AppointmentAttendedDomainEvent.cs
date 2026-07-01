using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Appointments.Events;

public sealed record AppointmentAttendedDomainEvent(
    Guid AppointmentId,
    Guid PatientId
) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

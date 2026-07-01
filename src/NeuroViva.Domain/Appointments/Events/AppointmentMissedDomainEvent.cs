using NeuroViva.Domain.Appointments.Enums;
using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Appointments.Events;

public sealed record AppointmentMissedDomainEvent(
    Guid AppointmentId,
    Guid PatientId,
    string AppointmentType,
    DateTime ScheduledAt,
    AppointmentMissReason Reason
) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

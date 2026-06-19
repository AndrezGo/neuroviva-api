using NeuroViva.Domain.Appointments.Enums;
using NeuroViva.Domain.Common;
using NeuroViva.Domain.Exceptions;

namespace NeuroViva.Domain.Appointments;

public sealed class Appointment : AggregateRoot<Guid>
{
    public Guid PatientId { get; private set; }
    public Guid? DoctorId { get; private set; }
    public AppointmentType Type { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Appointment() { }

    public static Appointment Schedule(
        Guid patientId, AppointmentType type, DateTime scheduledAt,
        string? notes = null, Guid? doctorId = null) => new()
    {
        Id = Guid.NewGuid(),
        PatientId = patientId,
        DoctorId = doctorId,
        Type = type,
        ScheduledAt = scheduledAt,
        Status = AppointmentStatus.Scheduled,
        Notes = notes,
        CreatedAt = DateTime.UtcNow
    };

    public void Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new BusinessRuleViolationException("appointment.invalid_transition",
                "Only scheduled appointments can be confirmed.");
        Status = AppointmentStatus.Confirmed;
    }

    public void Complete()
    {
        if (Status != AppointmentStatus.Confirmed && Status != AppointmentStatus.Scheduled)
            throw new BusinessRuleViolationException("appointment.invalid_transition",
                "Appointment cannot be completed in its current state.");
        Status = AppointmentStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed)
            throw new BusinessRuleViolationException("appointment.invalid_transition",
                "Completed appointments cannot be cancelled.");
        Status = AppointmentStatus.Cancelled;
    }
}

using NeuroViva.Domain.Common;
using NeuroViva.Domain.Medications.Events;

namespace NeuroViva.Domain.Medications;

public sealed class MedicationLog : Entity<Guid>
{
    public Guid MedicationId { get; private set; }
    public Guid LoggedBy { get; private set; }
    public DateTime LoggedAt { get; private set; }
    public bool Taken { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private MedicationLog() { }

    public static MedicationLog Record(
        Guid medicationId,
        Guid loggedBy,
        bool taken,
        Guid patientId,
        string medicationName,
        string? notes = null,
        DateTime? loggedAt = null)
    {
        var log = new MedicationLog
        {
            Id = Guid.NewGuid(),
            MedicationId = medicationId,
            LoggedBy = loggedBy,
            Taken = taken,
            Notes = notes,
            LoggedAt = loggedAt ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        if (!taken)
            log.RaiseEvent(new MedicationDoseSkippedDomainEvent(
                log.Id, patientId, medicationId, medicationName));

        return log;
    }
}

using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Medications;

public sealed class Medication : AggregateRoot<Guid>
{
    public Guid PatientId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Dose { get; private set; } = default!;
    public string Frequency { get; private set; } = default!;
    /// <summary>
    /// Structured dosing interval in hours, when the caregiver picked a fixed
    /// interval (e.g. "Cada 8 horas"). Null when Frequency is free text with
    /// no fixed interval (e.g. "1 vez al día", "según necesidad") — the
    /// "next dose" countdown is only computed when this is set.
    /// </summary>
    public int? IntervalHours { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<MedicationLog> _logs = new();
    public IReadOnlyCollection<MedicationLog> Logs => _logs.AsReadOnly();

    private Medication() { }

    public static Medication Prescribe(
        Guid patientId, string name, string dose,
        string frequency, DateOnly startDate, DateOnly? endDate = null,
        int? intervalHours = null) => new()
    {
        Id = Guid.NewGuid(),
        PatientId = patientId,
        Name = name,
        Dose = dose,
        Frequency = frequency,
        IntervalHours = intervalHours,
        StartDate = startDate,
        EndDate = endDate,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    public void Discontinue() => IsActive = false;
}

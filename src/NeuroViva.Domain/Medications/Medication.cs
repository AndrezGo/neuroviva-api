using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Medications;

public sealed class Medication : AggregateRoot<Guid>
{
    public Guid PatientId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Dose { get; private set; } = default!;
    public string Frequency { get; private set; } = default!;
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<MedicationLog> _logs = new();
    public IReadOnlyCollection<MedicationLog> Logs => _logs.AsReadOnly();

    private Medication() { }

    public static Medication Prescribe(
        Guid patientId, string name, string dose,
        string frequency, DateOnly startDate, DateOnly? endDate = null) => new()
    {
        Id = Guid.NewGuid(),
        PatientId = patientId,
        Name = name,
        Dose = dose,
        Frequency = frequency,
        StartDate = startDate,
        EndDate = endDate,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    public void Discontinue() => IsActive = false;
}

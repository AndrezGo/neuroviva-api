using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Patients;

public sealed class PatientDoctor : Entity<Guid>
{
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public bool IsActive { get; private set; }

    private PatientDoctor() { }

    public static PatientDoctor Assign(Guid patientId, Guid doctorId) => new()
    {
        Id = Guid.NewGuid(),
        PatientId = patientId,
        DoctorId = doctorId,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        IsActive = true
    };

    public void Deactivate() => IsActive = false;

    public void Reactivate()
    {
        IsActive = true;
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}

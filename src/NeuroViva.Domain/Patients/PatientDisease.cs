using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Patients;

public sealed class PatientDisease : Entity<Guid>
{
    public Guid PatientId { get; private set; }
    public Guid DiseaseId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    private PatientDisease() { }

    public static PatientDisease Assign(Guid patientId, Guid diseaseId) => new()
    {
        Id = Guid.NewGuid(),
        PatientId = patientId,
        DiseaseId = diseaseId,
        AssignedAt = DateTime.UtcNow
    };
}

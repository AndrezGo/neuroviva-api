namespace NeuroViva.Domain.Patients.Repositories;

public interface IPatientDiseaseRepository
{
    Task<IReadOnlyList<PatientDisease>> ListByPatientAsync(Guid patientId, CancellationToken ct = default);

    /// <summary>
    /// Replaces the full set of diseases assigned to a patient with <paramref name="diseaseIds"/>.
    /// Does not call SaveChanges — the caller persists via the same unit of work.
    /// </summary>
    Task ReplaceForPatientAsync(Guid patientId, IReadOnlyCollection<Guid> diseaseIds, CancellationToken ct = default);
}

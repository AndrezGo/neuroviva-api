namespace NeuroViva.Domain.Medications.Repositories;

public interface IMedicationRepository
{
    Task<Medication?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Medication>> ListActiveByPatientAsync(Guid patientId, CancellationToken ct = default);
    Task AddAsync(Medication medication, CancellationToken ct = default);
    void Update(Medication medication);
}

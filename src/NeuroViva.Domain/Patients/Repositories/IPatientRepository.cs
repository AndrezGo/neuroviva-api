namespace NeuroViva.Domain.Patients.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Patient>> ListByDoctorAsync(Guid doctorId, CancellationToken ct = default);
    Task<IReadOnlyList<Patient>> ListByCaregiverAsync(Guid caregiverId, CancellationToken ct = default);

    /// <summary>
    /// Looks up a patient by document number within a specific tenant.
    /// Normalizes <paramref name="documentNumber"/> to UPPER before querying.
    /// </summary>
    Task<Patient?> GetByDocumentNumberAsync(Guid tenantId, string documentNumber, CancellationToken ct = default);

    /// <summary>
    /// Looks up a patient by their linked user account.
    /// UserId is globally unique — no tenantId filter is applied.
    /// </summary>
    Task<Patient?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task AddAsync(Patient patient, CancellationToken ct = default);
    void Update(Patient patient);
}

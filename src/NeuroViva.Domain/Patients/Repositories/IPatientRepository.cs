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

    /// <summary>
    /// Cross-tenant lookup of a patient by document number, used by the ClaimProfile flow
    /// (document number acts as a shared code between caregiver and the patient he registered).
    /// Normalizes <paramref name="documentNumber"/> to UPPER before querying.
    /// Prioritizes candidates in this order:
    ///   (1) a patient already linked to <paramref name="preferredUserId"/> (idempotency),
    ///   (2) an unclaimed patient (UserId is null) — the invitation case,
    ///   (3) any other patient (already claimed by a different user — handler will surface conflict).
    /// Ties broken by CreatedAt ascending (oldest first).
    /// Returns null if no patient matches the document number in ANY tenant.
    /// </summary>
    Task<Patient?> FindClaimableByDocumentNumberAsync(string documentNumber, Guid preferredUserId, CancellationToken ct = default);

    Task AddAsync(Patient patient, CancellationToken ct = default);
    void Update(Patient patient);
}

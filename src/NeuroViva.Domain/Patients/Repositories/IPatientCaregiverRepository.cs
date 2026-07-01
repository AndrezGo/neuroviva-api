namespace NeuroViva.Domain.Patients.Repositories;

public interface IPatientCaregiverRepository
{
    /// <summary>
    /// Returns all PatientCaregiver rows for a given caregiver where the linked patient is active,
    /// ordered by start_date descending (most recent first).
    /// </summary>
    Task<IReadOnlyList<PatientCaregiverWithPatient>> GetActiveByCaregiverAsync(
        Guid caregiverId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the PatientCaregiver link for the specified patient/caregiver pair, or null if not found.
    /// </summary>
    Task<PatientCaregiver?> GetByPatientAndCaregiverAsync(
        Guid patientId,
        Guid caregiverId,
        CancellationToken ct = default);

    Task AddAsync(PatientCaregiver patientCaregiver, CancellationToken ct = default);
    void Update(PatientCaregiver patientCaregiver);
}

public sealed record PatientCaregiverWithPatient(
    PatientCaregiver Link,
    Patient Patient
);

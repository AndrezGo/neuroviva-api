namespace NeuroViva.Domain.Patients.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Patient>> ListByDoctorAsync(Guid doctorId, CancellationToken ct = default);
    Task<IReadOnlyList<Patient>> ListByCaregiverAsync(Guid caregiverId, CancellationToken ct = default);
    Task AddAsync(Patient patient, CancellationToken ct = default);
    void Update(Patient patient);
}

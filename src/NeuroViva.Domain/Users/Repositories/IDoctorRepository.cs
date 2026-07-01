namespace NeuroViva.Domain.Users.Repositories;

public interface IDoctorRepository
{
    Task<Doctor?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Doctor?> GetByMedicalLicenseAsync(string medicalLicense, CancellationToken ct = default);
    Task AddAsync(Doctor doctor, CancellationToken ct = default);
}

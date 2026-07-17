namespace NeuroViva.Domain.Patients.Repositories;

public interface IClinicalRecordRepository
{
    Task AddAsync(ClinicalRecord record, CancellationToken ct = default);
    Task<ClinicalRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

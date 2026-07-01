namespace NeuroViva.Domain.Patients.Repositories;

public interface IClinicalRecordRepository
{
    Task AddAsync(ClinicalRecord record, CancellationToken ct = default);
}

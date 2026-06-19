namespace NeuroViva.Domain.Medications.Repositories;

public interface IMedicationLogRepository
{
    Task AddAsync(MedicationLog log, CancellationToken ct = default);
}

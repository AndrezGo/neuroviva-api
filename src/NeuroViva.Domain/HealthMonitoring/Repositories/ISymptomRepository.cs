namespace NeuroViva.Domain.HealthMonitoring.Repositories;

public interface ISymptomRepository
{
    Task<Symptom?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Symptom>> ListByPatientAsync(Guid patientId, int limit = 50, CancellationToken ct = default);
    Task AddAsync(Symptom symptom, CancellationToken ct = default);
    void Update(Symptom symptom);
}

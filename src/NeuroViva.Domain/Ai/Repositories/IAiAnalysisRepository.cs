namespace NeuroViva.Domain.Ai.Repositories;

public interface IAiAnalysisRepository
{
    Task<AiAnalysis?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<AiAnalysis>> ListByPatientAsync(Guid patientId, CancellationToken ct = default);
    Task AddAsync(AiAnalysis analysis, CancellationToken ct = default);
}

public interface IAlertRepository
{
    Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Alert>> ListByDoctorAsync(Guid doctorId, bool includeResolved = false, CancellationToken ct = default);
    Task AddAsync(Alert alert, CancellationToken ct = default);
    void Update(Alert alert);
}

namespace NeuroViva.Domain.Content.Repositories;

public interface IScientificArticleRecordRepository
{
    Task UpsertManyAsync(IReadOnlyList<ScientificArticleRecord> articles, CancellationToken ct = default);
    Task<DateTime?> GetLastFetchedAtAsync(Guid diseaseId, CancellationToken ct = default);
    Task<IReadOnlyList<ScientificArticleRecord>> ListByDiseaseIdsAsync(
        IReadOnlyCollection<Guid> diseaseIds,
        DateTime sinceDate,
        CancellationToken ct = default);
}

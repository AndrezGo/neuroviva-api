namespace NeuroViva.Domain.Content.Repositories;

public interface IScientificArticleRecordRepository
{
    Task UpsertManyAsync(IReadOnlyList<ScientificArticleRecord> articles, CancellationToken ct = default);
    Task<DateTime?> GetLastFetchedAtAsync(Guid diseaseId, string language, CancellationToken ct = default);
    Task<IReadOnlyList<ScientificArticleRecord>> ListByDiseaseIdsAsync(
        IReadOnlyCollection<Guid> diseaseIds,
        string language,
        DateTime sinceDate,
        CancellationToken ct = default);
}

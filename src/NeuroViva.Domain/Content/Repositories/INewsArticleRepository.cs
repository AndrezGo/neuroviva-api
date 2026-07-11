namespace NeuroViva.Domain.Content.Repositories;

public interface INewsArticleRepository
{
    Task UpsertManyAsync(IReadOnlyList<NewsArticle> articles, CancellationToken ct = default);
    Task<DateTime?> GetLastFetchedAtAsync(Guid diseaseId, CancellationToken ct = default);
    Task<IReadOnlyList<NewsArticle>> ListByDiseaseIdsAsync(
        IReadOnlyCollection<Guid> diseaseIds,
        DateTime sinceDate,
        CancellationToken ct = default);
}

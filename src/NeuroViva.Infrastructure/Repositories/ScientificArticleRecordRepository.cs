using Microsoft.EntityFrameworkCore;
using NeuroViva.Application.Common.Exceptions;
using NeuroViva.Domain.Content;
using NeuroViva.Domain.Content.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class ScientificArticleRecordRepository : IScientificArticleRecordRepository
{
    private readonly NeuroVivaDbContext _db;

    public ScientificArticleRecordRepository(NeuroVivaDbContext db) => _db = db;

    public async Task UpsertManyAsync(IReadOnlyList<ScientificArticleRecord> articles, CancellationToken ct = default)
    {
        if (articles.Count == 0)
            return;

        var diseaseIds = articles.Select(a => a.DiseaseId).Distinct().ToList();
        var externalGuids = articles.Select(a => a.ExternalGuid).ToList();

        var existing = await _db.ScientificArticleRecords
            .AsNoTracking()
            .Where(a => diseaseIds.Contains(a.DiseaseId) && externalGuids.Contains(a.ExternalGuid))
            .Select(a => new { a.DiseaseId, a.ExternalGuid })
            .ToListAsync(ct);

        var existingSet = existing
            .Select(a => (a.DiseaseId, a.ExternalGuid))
            .ToHashSet();

        var newOnes = articles
            .Where(a => !existingSet.Contains((a.DiseaseId, a.ExternalGuid)))
            .ToList();

        if (newOnes.Count == 0)
            return;

        try
        {
            await _db.ScientificArticleRecords.AddRangeAsync(newOnes, ct);
            await _db.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException)
        {
            // Concurrent insert of same external_guid — swallow silently, data already exists.
        }
    }

    public async Task<DateTime?> GetLastFetchedAtAsync(Guid diseaseId, string language, CancellationToken ct = default)
        => await _db.ScientificArticleRecords
            .AsNoTracking()
            .Where(a => a.DiseaseId == diseaseId && a.Language == language)
            .MaxAsync(a => (DateTime?)a.FetchedAt, ct);

    public async Task<IReadOnlyList<ScientificArticleRecord>> ListByDiseaseIdsAsync(
        IReadOnlyCollection<Guid> diseaseIds,
        string language,
        DateTime sinceDate,
        CancellationToken ct = default)
        => await _db.ScientificArticleRecords
            .AsNoTracking()
            .Where(a => diseaseIds.Contains(a.DiseaseId) && a.Language == language && a.PublishedAt >= sinceDate)
            .OrderByDescending(a => a.PublishedAt)
            .ToListAsync(ct);
}

using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class ClinicalRecordRepository : IClinicalRecordRepository
{
    private readonly NeuroVivaDbContext _db;

    public ClinicalRecordRepository(NeuroVivaDbContext db) => _db = db;

    public async Task AddAsync(ClinicalRecord record, CancellationToken ct = default)
        => await _db.ClinicalRecords.AddAsync(record, ct);

    public Task<ClinicalRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.ClinicalRecords
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<ClinicalRecordAttachment>> GetPdfAttachmentsForBackfillAsync(
        int batchSize,
        CancellationToken ct = default)
    {
        // Returns tracked entities so callers can mutate them and call SaveChanges.
        return await _db.Set<ClinicalRecordAttachment>()
            .Where(a => a.ContentType == "application/pdf" && a.ExtractedText == null)
            .Take(batchSize)
            .ToListAsync(ct);
    }
}

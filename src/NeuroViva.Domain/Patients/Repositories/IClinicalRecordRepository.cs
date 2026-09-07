namespace NeuroViva.Domain.Patients.Repositories;

public interface IClinicalRecordRepository
{
    Task AddAsync(ClinicalRecord record, CancellationToken ct = default);
    Task<ClinicalRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns tracked PDF attachments whose ExtractedText has not yet been populated.
    /// Used exclusively by the admin backfill endpoint. Caller is responsible for SaveChanges.
    /// </summary>
    Task<IReadOnlyList<ClinicalRecordAttachment>> GetPdfAttachmentsForBackfillAsync(
        int batchSize,
        CancellationToken ct = default);
}

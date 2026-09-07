using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Patients;

public sealed class ClinicalRecordAttachment : Entity<Guid>
{
    public Guid ClinicalRecordId { get; private set; }
    public string StoragePath { get; private set; } = default!;
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long? FileSizeBytes { get; private set; }
    public Guid UploadedBy { get; private set; }
    public DateTime UploadedAt { get; private set; }

    /// <summary>
    /// Plain text extracted from the file at upload time (PDF only).
    /// Null when extraction is not applicable (image), was not possible (encrypted/scanned PDF),
    /// or when the record predates extraction support (backfill pending).
    /// Persisted up to 6000 characters — sufficient to cover typical lab reports (2-5 pages,
    /// ~3-5k chars) without inflating storage for pathological cases.
    /// </summary>
    public string? ExtractedText { get; private set; }

    private ClinicalRecordAttachment() { }

    public static ClinicalRecordAttachment Create(
        Guid clinicalRecordId,
        string storagePath,
        string fileName,
        string contentType,
        long? fileSizeBytes,
        Guid uploadedBy,
        string? extractedText = null,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            throw new ArgumentException("Storage path must not be empty.", nameof(storagePath));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name must not be empty.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type must not be empty.", nameof(contentType));

        return new ClinicalRecordAttachment
        {
            Id = id ?? Guid.NewGuid(),
            ClinicalRecordId = clinicalRecordId,
            StoragePath = storagePath,
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow,
            ExtractedText = extractedText
        };
    }

    /// <summary>
    /// Updates the extracted text after upload (used by the admin backfill endpoint).
    /// Replaces any existing value; pass null to clear.
    /// </summary>
    public void SetExtractedText(string? text) => ExtractedText = text;
}

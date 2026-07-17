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

    private ClinicalRecordAttachment() { }

    public static ClinicalRecordAttachment Create(
        Guid clinicalRecordId,
        string storagePath,
        string fileName,
        string contentType,
        long? fileSizeBytes,
        Guid uploadedBy,
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
            UploadedAt = DateTime.UtcNow
        };
    }
}

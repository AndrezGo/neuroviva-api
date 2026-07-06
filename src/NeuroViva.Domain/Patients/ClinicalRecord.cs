using NeuroViva.Domain.Common;
using NeuroViva.Domain.Patients.Enums;
using System.Text.Json;

namespace NeuroViva.Domain.Patients;

public sealed class ClinicalRecord : Entity<Guid>
{
    public Guid PatientId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public ClinicalEventType EventType { get; private set; }
    public string Description { get; private set; } = default!;
    public DateTime EventDate { get; private set; }
    public JsonDocument? Metadata { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public string? AttachmentPath { get; private set; }
    public string? AttachmentFileName { get; private set; }
    public string? AttachmentContentType { get; private set; }

    private ClinicalRecord() { }

    public static ClinicalRecord Create(
        Guid patientId,
        Guid createdBy,
        ClinicalEventType eventType,
        string description,
        DateTime? eventDate = null,
        JsonDocument? metadata = null,
        Guid? id = null,
        string? attachmentPath = null,
        string? attachmentFileName = null,
        string? attachmentContentType = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        PatientId = patientId,
        CreatedBy = createdBy,
        EventType = eventType,
        Description = description,
        EventDate = eventDate ?? DateTime.UtcNow,
        Metadata = metadata,
        CreatedAt = DateTime.UtcNow,
        AttachmentPath = attachmentPath,
        AttachmentFileName = attachmentFileName,
        AttachmentContentType = attachmentContentType
    };

    /// <summary>
    /// Attaches a file to the record. Throws if already attached or if any argument is empty.
    /// </summary>
    public void AttachFile(string path, string fileName, string contentType)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Attachment path must not be empty.", nameof(path));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Attachment file name must not be empty.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Attachment content type must not be empty.", nameof(contentType));
        if (AttachmentPath is not null)
            throw new InvalidOperationException("An attachment is already associated with this clinical record.");

        AttachmentPath = path;
        AttachmentFileName = fileName;
        AttachmentContentType = contentType;
    }
}

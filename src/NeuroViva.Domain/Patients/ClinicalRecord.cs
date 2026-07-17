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

    private readonly List<ClinicalRecordAttachment> _attachments = new();
    public IReadOnlyCollection<ClinicalRecordAttachment> Attachments => _attachments.AsReadOnly();

    private ClinicalRecord() { }

    public static ClinicalRecord Create(
        Guid patientId,
        Guid createdBy,
        ClinicalEventType eventType,
        string description,
        DateTime? eventDate = null,
        JsonDocument? metadata = null,
        Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        PatientId = patientId,
        CreatedBy = createdBy,
        EventType = eventType,
        Description = description,
        EventDate = eventDate ?? DateTime.UtcNow,
        Metadata = metadata,
        CreatedAt = DateTime.UtcNow
    };

    public void AddAttachment(
        string storagePath,
        string fileName,
        string contentType,
        long? fileSizeBytes,
        Guid uploadedBy)
    {
        var attachment = ClinicalRecordAttachment.Create(
            clinicalRecordId: this.Id,
            storagePath: storagePath,
            fileName: fileName,
            contentType: contentType,
            fileSizeBytes: fileSizeBytes,
            uploadedBy: uploadedBy);

        _attachments.Add(attachment);
    }
}

using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Common.Options;
using NeuroViva.Application.Common.Services;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Enums;
using NeuroViva.Domain.Patients.Repositories;

namespace NeuroViva.Application.MedicalRecords.Commands.UploadClinicalNote;

public sealed class UploadClinicalNoteCommandHandler
    : IRequestHandler<UploadClinicalNoteCommand, Result<UploadClinicalNoteResult>>
{
    private readonly IPatientAccessGuard _guard;
    private readonly IClinicalRecordRepository _clinicalRecordRepo;
    private readonly IUnitOfWork _uow;
    private readonly IStorageService _storageService;
    private readonly StorageOptions _storageOptions;
    private readonly ICurrentUserService _currentUser;

    public UploadClinicalNoteCommandHandler(
        IPatientAccessGuard guard,
        IClinicalRecordRepository clinicalRecordRepo,
        IUnitOfWork uow,
        IStorageService storageService,
        StorageOptions storageOptions,
        ICurrentUserService currentUser)
    {
        _guard = guard;
        _clinicalRecordRepo = clinicalRecordRepo;
        _uow = uow;
        _storageService = storageService;
        _storageOptions = storageOptions;
        _currentUser = currentUser;
    }

    public async Task<Result<UploadClinicalNoteResult>> Handle(
        UploadClinicalNoteCommand request,
        CancellationToken cancellationToken)
    {
        // Guard first — authorization before any DB read/write
        var guardResult = await _guard.ResolveAndAuthorizeAsync(request.PatientId, cancellationToken);
        if (guardResult.IsFailure)
            return guardResult.Error;

        var patientId = guardResult.Value;

        // Per-attachment validation
        foreach (var attachment in request.Attachments)
        {
            if (attachment.Bytes.Length > AttachmentValidation.MaxAttachmentBytes)
                return Error.Validation(
                    "attachment.too_large",
                    $"Attachment '{attachment.FileName}' exceeds the {AttachmentValidation.MaxAttachmentBytes / (1024 * 1024)} MB maximum allowed size.");

            if (!AttachmentValidation.AllowedContentTypes.Contains(attachment.ContentType))
                return Error.Validation(
                    "attachment.type_not_allowed",
                    $"Attachment content type '{attachment.ContentType}' is not allowed. Allowed types: image/jpeg, image/png, image/webp, application/pdf.");
        }

        var eventType = MapEventType(request.EventType);

        // Pre-generate record Id so we can build storage paths before persisting
        var recordId = Guid.NewGuid();

        var record = ClinicalRecord.Create(
            patientId: patientId,
            createdBy: _currentUser.UserId!.Value,
            eventType: eventType,
            description: request.Description.Trim(),
            eventDate: request.EventDate,
            metadata: null,
            id: recordId);

        // Upload attachments BEFORE saving to DB
        foreach (var attachment in request.Attachments)
        {
            var attachmentId = Guid.NewGuid();
            var sanitizedFileName = AttachmentValidation.SanitizeFileName(attachment.FileName);
            var storagePath = $"clinical-records/{patientId}/{recordId}/{attachmentId}/{sanitizedFileName}";

            using var stream = new MemoryStream(attachment.Bytes, writable: false);
            await _storageService.UploadAsync(
                _storageOptions.AttachmentsBucket,
                storagePath,
                stream,
                attachment.ContentType,
                cancellationToken);

            record.AddAttachment(
                storagePath: storagePath,
                fileName: sanitizedFileName,
                contentType: attachment.ContentType,
                fileSizeBytes: attachment.Bytes.Length,
                uploadedBy: _currentUser.UserId!.Value);
        }

        await _clinicalRecordRepo.AddAsync(record, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new UploadClinicalNoteResult(record.Id);
    }

    private static ClinicalEventType MapEventType(string raw) =>
        raw.Trim().ToLowerInvariant() switch
        {
            "consultation" => ClinicalEventType.Consultation,
            "note" => ClinicalEventType.Note,
            "other" => ClinicalEventType.Other,
            _ => ClinicalEventType.Note
        };
}

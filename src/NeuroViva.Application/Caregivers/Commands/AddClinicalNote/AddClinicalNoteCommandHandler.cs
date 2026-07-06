using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Common.Options;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Enums;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Commands.AddClinicalNote;

public sealed class AddClinicalNoteCommandHandler
    : IRequestHandler<AddClinicalNoteCommand, Result<AddClinicalNoteResult>>
{
    private const int MaxAttachmentBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf"
    };

    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IClinicalRecordRepository _clinicalRecordRepo;
    private readonly IUnitOfWork _uow;
    private readonly IStorageService _storageService;
    private readonly StorageOptions _storageOptions;

    public AddClinicalNoteCommandHandler(
        ICurrentUserService currentUser,
        ICaregiverRepository caregiverRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        IClinicalRecordRepository clinicalRecordRepo,
        IUnitOfWork uow,
        IStorageService storageService,
        StorageOptions storageOptions)
    {
        _currentUser = currentUser;
        _caregiverRepo = caregiverRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _clinicalRecordRepo = clinicalRecordRepo;
        _uow = uow;
        _storageService = storageService;
        _storageOptions = storageOptions;
    }

    public async Task<Result<AddClinicalNoteResult>> Handle(
        AddClinicalNoteCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (string.IsNullOrWhiteSpace(request.Description))
            return Error.Validation("clinical_note.description_required", "Description is required");

        // Validate attachment if present
        if (request.AttachmentBytes is not null)
        {
            if (request.AttachmentBytes.Length > MaxAttachmentBytes)
                return Error.Validation(
                    "clinical_note.attachment_too_large",
                    "Attachment exceeds the 10 MB maximum allowed size.");

            var contentType = request.AttachmentContentType ?? string.Empty;
            if (!AllowedContentTypes.Contains(contentType))
                return Error.Validation(
                    "clinical_note.attachment_type_not_allowed",
                    $"Attachment content type '{contentType}' is not allowed. Allowed types: image/jpeg, image/png, image/webp, application/pdf.");
        }

        // Resolve caregiver profile
        var caregiver = await _caregiverRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (caregiver is null)
            return Error.NotFound("caregiver.not_found", "Caregiver profile not found");

        // Resolve linked patient — take first active link (most recent by start_date)
        var links = await _patientCaregiverRepo.GetActiveByCaregiverAsync(caregiver.Id, cancellationToken);
        var link = links.FirstOrDefault();
        if (link is null)
            return Error.NotFound("caregiver.no_patient", "Caregiver has no linked patient");

        var eventType = MapEventType(request.EventType);
        var patientId = link.Patient.Id;

        // Pre-generate record Id so we can build the storage path before persisting
        var recordId = Guid.NewGuid();

        string? attachmentPath = null;
        string? sanitizedFileName = null;

        if (request.AttachmentBytes is not null)
        {
            sanitizedFileName = SanitizeFileName(request.AttachmentFileName);
            attachmentPath = $"clinical-records/{patientId}/{recordId}/{sanitizedFileName}";

            using var stream = new MemoryStream(request.AttachmentBytes, writable: false);
            // Upload BEFORE saving to DB — if upload fails, no orphan DB row is created
            await _storageService.UploadAsync(
                _storageOptions.AttachmentsBucket,
                attachmentPath,
                stream,
                request.AttachmentContentType!,
                cancellationToken);
        }

        var record = ClinicalRecord.Create(
            patientId: patientId,
            createdBy: _currentUser.UserId.Value,
            eventType: eventType,
            description: request.Description.Trim(),
            eventDate: request.EventDate,
            metadata: null,
            id: recordId,
            attachmentPath: attachmentPath,
            attachmentFileName: sanitizedFileName,
            attachmentContentType: request.AttachmentBytes is not null ? request.AttachmentContentType : null);

        await _clinicalRecordRepo.AddAsync(record, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new AddClinicalNoteResult(record.Id);
    }

    /// <summary>
    /// Strips path separators and keeps only alphanumerics, dots, dashes and underscores.
    /// Falls back to "attachment.bin" if the result is empty.
    /// </summary>
    private static string SanitizeFileName(string? rawFileName)
    {
        if (string.IsNullOrWhiteSpace(rawFileName))
            return "attachment.bin";

        // Take only the file name part (ignore any directory prefix the client may have sent)
        var name = Path.GetFileName(rawFileName);

        // Keep only safe characters
        var safe = new string(name.Where(c =>
            char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_').ToArray());

        return string.IsNullOrWhiteSpace(safe) ? "attachment.bin" : safe;
    }

    /// <summary>
    /// Maps a free-text event type (Spanish or English) to the ClinicalEventType enum.
    /// Defaults to Other when unrecognized.
    /// </summary>
    private static ClinicalEventType MapEventType(string raw) =>
        raw.Trim().ToLowerInvariant() switch
        {
            "consultation" or "consulta" => ClinicalEventType.Consultation,
            "exam" or "examen" => ClinicalEventType.Exam,
            "note" or "nota" => ClinicalEventType.Note,
            _ => ClinicalEventType.Other
        };
}

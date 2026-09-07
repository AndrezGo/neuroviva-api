using MediatR;
using Microsoft.Extensions.Logging;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Common.Options;
using NeuroViva.Application.Common.Services;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Enums;
using NeuroViva.Domain.Patients.Repositories;

namespace NeuroViva.Application.MedicalRecords.Commands.UploadExam;

public sealed class UploadExamCommandHandler
    : IRequestHandler<UploadExamCommand, Result<UploadExamResult>>
{
    private readonly IPatientAccessGuard _guard;
    private readonly IClinicalRecordRepository _clinicalRecordRepo;
    private readonly IUnitOfWork _uow;
    private readonly IStorageService _storageService;
    private readonly StorageOptions _storageOptions;
    private readonly ICurrentUserService _currentUser;
    private readonly IPdfTextExtractor _pdfExtractor;
    private readonly ILogger<UploadExamCommandHandler> _logger;

    public UploadExamCommandHandler(
        IPatientAccessGuard guard,
        IClinicalRecordRepository clinicalRecordRepo,
        IUnitOfWork uow,
        IStorageService storageService,
        StorageOptions storageOptions,
        ICurrentUserService currentUser,
        IPdfTextExtractor pdfExtractor,
        ILogger<UploadExamCommandHandler> logger)
    {
        _guard = guard;
        _clinicalRecordRepo = clinicalRecordRepo;
        _uow = uow;
        _storageService = storageService;
        _storageOptions = storageOptions;
        _currentUser = currentUser;
        _pdfExtractor = pdfExtractor;
        _logger = logger;
    }

    public async Task<Result<UploadExamResult>> Handle(
        UploadExamCommand request,
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

        // Pre-generate record Id so we can build storage paths before persisting
        var recordId = Guid.NewGuid();

        var record = ClinicalRecord.Create(
            patientId: patientId,
            createdBy: _currentUser.UserId!.Value,
            eventType: ClinicalEventType.Exam,
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

            // Extract text after upload, before persisting. Never lets extraction failures
            // abort the upload — defence in depth on top of TryExtractText's own catch.
            string? extractedText = null;
            if (attachment.ContentType == "application/pdf")
            {
                try
                {
                    extractedText = _pdfExtractor.TryExtractText(attachment.Bytes);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Unexpected exception from IPdfTextExtractor for file '{FileName}'. ExtractedText will be null.",
                        sanitizedFileName);
                }
            }

            record.AddAttachment(
                storagePath: storagePath,
                fileName: sanitizedFileName,
                contentType: attachment.ContentType,
                fileSizeBytes: attachment.Bytes.Length,
                uploadedBy: _currentUser.UserId!.Value,
                extractedText: extractedText);
        }

        await _clinicalRecordRepo.AddAsync(record, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new UploadExamResult(record.Id);
    }
}

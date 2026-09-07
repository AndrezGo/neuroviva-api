using MediatR;
using Microsoft.Extensions.Logging;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Common.Options;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Patients.Repositories;

namespace NeuroViva.Application.Admin.Commands.BackfillPdfExtraction;

public sealed class BackfillPdfExtractionCommandHandler
    : IRequestHandler<BackfillPdfExtractionCommand, Result<BackfillPdfExtractionResult>>
{
    private readonly IClinicalRecordRepository _clinicalRecordRepo;
    private readonly IUnitOfWork _uow;
    private readonly IStorageService _storageService;
    private readonly StorageOptions _storageOptions;
    private readonly IPdfTextExtractor _pdfExtractor;
    private readonly ILogger<BackfillPdfExtractionCommandHandler> _logger;

    public BackfillPdfExtractionCommandHandler(
        IClinicalRecordRepository clinicalRecordRepo,
        IUnitOfWork uow,
        IStorageService storageService,
        StorageOptions storageOptions,
        IPdfTextExtractor pdfExtractor,
        ILogger<BackfillPdfExtractionCommandHandler> logger)
    {
        _clinicalRecordRepo = clinicalRecordRepo;
        _uow = uow;
        _storageService = storageService;
        _storageOptions = storageOptions;
        _pdfExtractor = pdfExtractor;
        _logger = logger;
    }

    public async Task<Result<BackfillPdfExtractionResult>> Handle(
        BackfillPdfExtractionCommand request,
        CancellationToken cancellationToken)
    {
        var batchSize = request.BatchSize is > 0 and <= 1000 ? request.BatchSize : 200;

        var attachments = await _clinicalRecordRepo.GetPdfAttachmentsForBackfillAsync(
            batchSize, cancellationToken);

        var total = attachments.Count;
        var processed = 0;
        var failed = 0;

        _logger.LogInformation(
            "PDF backfill starting. Found {Total} PDF attachment(s) without extracted text (batch limit: {BatchSize}).",
            total, batchSize);

        foreach (var attachment in attachments)
        {
            try
            {
                var bytes = await _storageService.DownloadAsync(
                    _storageOptions.AttachmentsBucket,
                    attachment.StoragePath,
                    cancellationToken);

                var extractedText = _pdfExtractor.TryExtractText(bytes);
                // Use a sentinel so rows where extraction yields nothing are not re-attempted each run.
                // We store an empty string instead of null to distinguish "tried and got nothing" from "not yet tried".
                // Actually per the architecture brief, null means "not yet attempted", so we leave null when extraction fails.
                // If extraction succeeds but is empty text, we also leave null (image PDF).
                attachment.SetExtractedText(extractedText);

                processed++;
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogWarning(ex,
                    "Failed to backfill PDF extraction for attachment {AttachmentId} (path: '{StoragePath}'). Skipping.",
                    attachment.Id,
                    attachment.StoragePath);
            }
        }

        if (processed > 0)
            await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "PDF backfill complete. Total: {Total}, Processed: {Processed}, Failed: {Failed}.",
            total, processed, failed);

        return new BackfillPdfExtractionResult(
            Total: total,
            Processed: processed,
            Failed: failed);
    }
}

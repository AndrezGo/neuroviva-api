using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Admin.Commands.BackfillPdfExtraction;

/// <summary>
/// One-shot admin command: iterates clinical_record_attachment rows where
/// content_type = 'application/pdf' AND extracted_text IS NULL, downloads each file
/// from storage, extracts text, and persists the result.
/// </summary>
public sealed record BackfillPdfExtractionCommand(int BatchSize = 200)
    : IRequest<Result<BackfillPdfExtractionResult>>;

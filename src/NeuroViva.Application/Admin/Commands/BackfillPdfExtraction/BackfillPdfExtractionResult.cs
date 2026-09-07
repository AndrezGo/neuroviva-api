namespace NeuroViva.Application.Admin.Commands.BackfillPdfExtraction;

public sealed record BackfillPdfExtractionResult(
    int Total,
    int Processed,
    int Failed);

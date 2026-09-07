using System.Text;
using Microsoft.Extensions.Logging;
using NeuroViva.Application.Common.Abstractions;
using UglyToad.PdfPig;

namespace NeuroViva.Infrastructure.Pdf;

public sealed class PdfPigTextExtractor : IPdfTextExtractor
{
    /// <summary>
    /// Maximum characters persisted per attachment.
    /// Buffer covers typical lab reports (2-5 pages, ~3-5k chars) without inflating storage
    /// for pathological cases (200-page documents).
    /// </summary>
    private const int MaxExtractedCharacters = 6000;

    private readonly ILogger<PdfPigTextExtractor> _logger;

    public PdfPigTextExtractor(ILogger<PdfPigTextExtractor> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string? TryExtractText(byte[] pdfBytes)
    {
        try
        {
            var sb = new StringBuilder();

            using var document = PdfDocument.Open(pdfBytes);
            foreach (var page in document.GetPages())
            {
                sb.Append(page.Text);
            }

            var text = sb.ToString().Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                // Likely a scanned image-only PDF with no embedded text layer
                _logger.LogWarning(
                    "PdfPig extracted no text from PDF ({SizeBytes} bytes). Possibly scanned image-only or unsupported encoding.",
                    pdfBytes.Length);
                return null;
            }

            if (text.Length > MaxExtractedCharacters)
            {
                _logger.LogDebug(
                    "Extracted PDF text truncated from {Original} to {Max} characters.",
                    text.Length,
                    MaxExtractedCharacters);
                return text[..MaxExtractedCharacters];
            }

            return text;
        }
        catch (Exception ex)
        {
            // Short hash for correlation without logging file contents
            var hashHint = Convert.ToHexString(pdfBytes.Length >= 4
                ? pdfBytes[..4]
                : pdfBytes).ToLowerInvariant();

            _logger.LogWarning(ex,
                "PdfPig failed to extract text from PDF (first-bytes hint: {HashHint}, size: {SizeBytes}). " +
                "Likely encrypted, corrupted, or in an unsupported format. ExtractedText will be null.",
                hashHint,
                pdfBytes.Length);

            return null;
        }
    }
}

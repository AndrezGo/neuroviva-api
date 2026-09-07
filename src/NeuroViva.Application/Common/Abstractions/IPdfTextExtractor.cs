namespace NeuroViva.Application.Common.Abstractions;

public interface IPdfTextExtractor
{
    /// <summary>
    /// Extracts plain text from a PDF byte array.
    /// Returns null if extraction fails or yields no meaningful text.
    /// Never throws for expected failure cases (encrypted, corrupted, scanned image-only PDFs).
    /// </summary>
    string? TryExtractText(byte[] pdfBytes);
}

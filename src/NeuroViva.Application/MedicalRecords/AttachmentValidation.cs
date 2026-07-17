namespace NeuroViva.Application.MedicalRecords;

public static class AttachmentValidation
{
    public const int MaxAttachmentBytes = 10 * 1024 * 1024;
    public const int MaxAttachmentsPerRecord = 5;

    public static readonly IReadOnlySet<string> AllowedContentTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "application/pdf"
        };

    /// <summary>
    /// Strips path separators and keeps only alphanumerics, dots, dashes and underscores.
    /// Falls back to "attachment.bin" if the result is empty.
    /// </summary>
    public static string SanitizeFileName(string? rawFileName)
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
}

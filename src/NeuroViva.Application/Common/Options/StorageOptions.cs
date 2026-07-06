namespace NeuroViva.Application.Common.Options;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Supabase";
    public string AttachmentsBucket { get; set; } = "neuroviva-clinical-attachments";
    public int SignedUrlExpirySeconds { get; set; } = 3600;
}

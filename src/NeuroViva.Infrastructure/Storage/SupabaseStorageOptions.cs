namespace NeuroViva.Infrastructure.Storage;

public sealed class SupabaseStorageOptions
{
    public const string SectionName = "Supabase";

    public string Url { get; set; } = default!;
    public string ServiceRoleKey { get; set; } = default!;
}

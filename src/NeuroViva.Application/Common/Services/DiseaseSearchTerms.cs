namespace NeuroViva.Application.Common.Services;

public static class DiseaseSearchTerms
{
    public static readonly IReadOnlyDictionary<string, string> BySlug =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "alzheimer",    "Alzheimer" },
            { "parkinson",    "Parkinson" },
            { "dementia_mci", "Demencia" },
            { "als",          "ELA esclerosis lateral amiotrófica" },
            { "huntington",   "Huntington" },
        };

    public static bool TryGetSearchTerm(string slug, out string term)
        => BySlug.TryGetValue(slug, out term!);
}

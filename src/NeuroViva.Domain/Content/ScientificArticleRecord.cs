using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Content;

public sealed class ScientificArticleRecord : Entity<Guid>
{
    public Guid DiseaseId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? SourceName { get; private set; }
    public string SourceUrl { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? Authors { get; private set; }
    public DateTime PublishedAt { get; private set; }
    public DateTime FetchedAt { get; private set; }
    public string ExternalGuid { get; private set; } = default!;
    public string Language { get; private set; } = default!;

    private ScientificArticleRecord() { }

    public static ScientificArticleRecord Create(
        Guid diseaseId,
        string title,
        string sourceUrl,
        string? sourceName,
        string? description,
        string? authors,
        DateTime publishedAt,
        string externalGuid,
        string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language cannot be null or whitespace.", nameof(language));

        return new ScientificArticleRecord
        {
            Id = Guid.NewGuid(),
            DiseaseId = diseaseId,
            Title = title,
            SourceUrl = sourceUrl,
            SourceName = sourceName,
            Description = description,
            Authors = authors,
            PublishedAt = publishedAt,
            FetchedAt = DateTime.UtcNow,
            ExternalGuid = externalGuid,
            Language = language,
        };
    }
}

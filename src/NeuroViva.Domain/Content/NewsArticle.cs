using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Content;

public sealed class NewsArticle : Entity<Guid>
{
    public Guid DiseaseId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? SourceName { get; private set; }
    public string SourceUrl { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateTime PublishedAt { get; private set; }
    public DateTime FetchedAt { get; private set; }
    public string ExternalGuid { get; private set; } = default!;

    private NewsArticle() { }

    public static NewsArticle Create(
        Guid diseaseId,
        string title,
        string sourceUrl,
        string? sourceName,
        string? description,
        DateTime publishedAt,
        string externalGuid) => new()
    {
        Id = Guid.NewGuid(),
        DiseaseId = diseaseId,
        Title = title,
        SourceUrl = sourceUrl,
        SourceName = sourceName,
        Description = description,
        PublishedAt = publishedAt,
        FetchedAt = DateTime.UtcNow,
        ExternalGuid = externalGuid,
    };
}

namespace NeuroViva.Application.Common.Abstractions;

public sealed record RawNewsItem(
    string Title,
    string Link,
    DateTime PublishedAt,
    string? Description,
    string? SourceName,
    string ExternalGuid);

public interface IGoogleNewsRssService
{
    Task<IReadOnlyList<RawNewsItem>> SearchAsync(string query, CancellationToken ct = default);
}

namespace NeuroViva.Application.Common.Abstractions;

public sealed record RawScientificArticle(
    string Title,
    string Link,
    DateTime PublishedAt,
    string? Description,
    string? SourceName,
    string? Authors,
    string ExternalGuid);

public interface IEuropePmcService
{
    Task<IReadOnlyList<RawScientificArticle>> SearchAsync(string query, string language, CancellationToken ct = default);
}

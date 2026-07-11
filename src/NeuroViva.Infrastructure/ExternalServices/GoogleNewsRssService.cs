using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using NeuroViva.Application.Common.Abstractions;

namespace NeuroViva.Infrastructure.ExternalServices;

public sealed class GoogleNewsRssService : IGoogleNewsRssService
{
    private readonly HttpClient _http;
    private readonly ILogger<GoogleNewsRssService> _logger;

    public GoogleNewsRssService(HttpClient http, ILogger<GoogleNewsRssService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RawNewsItem>> SearchAsync(string query, CancellationToken ct = default)
    {
        try
        {
            var relativeUrl = $"rss/search?q={Uri.EscapeDataString(query)}&hl=es-419&gl=CO&ceid=CO:es-419";

            var response = await _http.GetAsync(relativeUrl, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Google News RSS returned non-success status {StatusCode} for query '{Query}'.",
                    (int)response.StatusCode,
                    query);
                return Array.Empty<RawNewsItem>();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var doc = XDocument.Load(stream);

            var channel = doc.Root?.Element("channel");
            if (channel is null)
                return Array.Empty<RawNewsItem>();

            var results = new List<RawNewsItem>();

            foreach (var item in channel.Elements("item"))
            {
                var title = item.Element("title")?.Value;
                if (string.IsNullOrWhiteSpace(title))
                    continue;

                var link = item.Element("link")?.Value;
                if (string.IsNullOrWhiteSpace(link))
                    continue;

                var pubDateStr = item.Element("pubDate")?.Value;
                DateTime publishedAt;
                if (!string.IsNullOrWhiteSpace(pubDateStr)
                    && DateTimeOffset.TryParse(
                        pubDateStr,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var dto))
                {
                    publishedAt = dto.UtcDateTime;
                }
                else
                {
                    publishedAt = DateTime.UtcNow;
                }

                var description = item.Element("description")?.Value;
                var sourceName = item.Element("source")?.Value;
                var guid = item.Element("guid")?.Value;
                if (string.IsNullOrWhiteSpace(guid))
                    guid = link;

                results.Add(new RawNewsItem(title, link, publishedAt, description, sourceName, guid));
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to fetch or parse Google News RSS for query '{Query}'.",
                query);
            return Array.Empty<RawNewsItem>();
        }
    }
}

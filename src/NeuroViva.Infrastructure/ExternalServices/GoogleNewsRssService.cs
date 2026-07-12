using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml;
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
        var stopwatch = Stopwatch.StartNew();
        var relativeUrl = $"rss/search?q={Uri.EscapeDataString(query)}&hl=es-419&gl=CO&ceid=CO:es-419";
        _logger.LogInformation(
            "Google News RSS request starting. BaseAddress='{BaseAddress}', RelativeUrl='{RelativeUrl}', Query='{Query}'.",
            _http.BaseAddress?.ToString() ?? "(null)",
            relativeUrl,
            query);

        try
        {
            var response = await _http.GetAsync(relativeUrl, ct);

            var statusCode = (int)response.StatusCode;
            long? contentLength = response.Content.Headers.ContentLength;

            if (!response.IsSuccessStatusCode)
            {
                string bodyPreview;
                try
                {
                    var body = await response.Content.ReadAsStringAsync(ct);
                    bodyPreview = body.Substring(0, Math.Min(500, body.Length));
                }
                catch
                {
                    bodyPreview = "could not read body preview";
                }

                _logger.LogWarning(
                    "Google News RSS returned non-success status {StatusCode} after {ElapsedMs} ms. ContentLength={ContentLength}, Query='{Query}', BodyPreview='{BodyPreview}'.",
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    contentLength,
                    query,
                    bodyPreview);
                return Array.Empty<RawNewsItem>();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var doc = XDocument.Load(stream);

            var channel = doc.Root?.Element("channel");
            if (channel is null)
            {
                _logger.LogWarning(
                    "Google News RSS response had no <channel> element. StatusCode={StatusCode}, ContentLength={ContentLength}, ElapsedMs={ElapsedMs}, Query='{Query}'.",
                    statusCode,
                    contentLength,
                    stopwatch.ElapsedMilliseconds,
                    query);
                return Array.Empty<RawNewsItem>();
            }

            var results = new List<RawNewsItem>();
            var itemNodeCount = 0;

            foreach (var item in channel.Elements("item"))
            {
                itemNodeCount++;

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

                var description = StripHtml(item.Element("description")?.Value);
                var sourceName = item.Element("source")?.Value;
                var guid = item.Element("guid")?.Value;
                if (string.IsNullOrWhiteSpace(guid))
                    guid = link;

                results.Add(new RawNewsItem(title, link, publishedAt, description, sourceName, guid));
            }

            _logger.LogInformation(
                "Google News RSS request succeeded. StatusCode={StatusCode}, ContentLength={ContentLength}, ElapsedMs={ElapsedMs}, Query='{Query}', ItemNodes={ItemNodes}, Results={Results}.",
                statusCode,
                contentLength,
                stopwatch.ElapsedMilliseconds,
                query,
                itemNodeCount,
                results.Count);

            return results;
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(
                ex,
                "Google News RSS request timed out after {ElapsedMs} ms for query '{Query}'.",
                stopwatch.ElapsedMilliseconds,
                query);
            return Array.Empty<RawNewsItem>();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "Google News RSS request canceled by caller after {ElapsedMs} ms for query '{Query}'.",
                stopwatch.ElapsedMilliseconds,
                query);
            return Array.Empty<RawNewsItem>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "Google News RSS HTTP request failed after {ElapsedMs} ms for query '{Query}'. InnerStatus={InnerStatus}.",
                stopwatch.ElapsedMilliseconds,
                query,
                ex.StatusCode);
            return Array.Empty<RawNewsItem>();
        }
        catch (XmlException ex)
        {
            _logger.LogWarning(
                ex,
                "Google News RSS returned malformed XML after {ElapsedMs} ms for query '{Query}'.",
                stopwatch.ElapsedMilliseconds,
                query);
            return Array.Empty<RawNewsItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Google News RSS unexpected failure after {ElapsedMs} ms for query '{Query}'.",
                stopwatch.ElapsedMilliseconds,
                query);
            return Array.Empty<RawNewsItem>();
        }
    }

    private static string? StripHtml(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var stripped = Regex.Replace(value, "<[^>]+>", string.Empty);
        var decoded = WebUtility.HtmlDecode(stripped).Trim();
        return decoded.Length == 0 ? null : decoded;
    }
}

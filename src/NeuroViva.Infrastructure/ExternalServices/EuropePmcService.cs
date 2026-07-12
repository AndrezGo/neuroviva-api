using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NeuroViva.Application.Common.Abstractions;

namespace NeuroViva.Infrastructure.ExternalServices;

public sealed class EuropePmcService : IEuropePmcService
{
    private readonly HttpClient _http;
    private readonly ILogger<EuropePmcService> _logger;

    public EuropePmcService(HttpClient http, ILogger<EuropePmcService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RawScientificArticle>> SearchAsync(string query, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var relativeUrl = $"search?query={Uri.EscapeDataString(query)}&format=json&pageSize=20&resultType=core";
        _logger.LogInformation(
            "Europe PMC request starting. BaseAddress='{BaseAddress}', RelativeUrl='{RelativeUrl}', Query='{Query}'.",
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
                    "Europe PMC returned non-success status {StatusCode} after {ElapsedMs} ms. ContentLength={ContentLength}, Query='{Query}', BodyPreview='{BodyPreview}'.",
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    contentLength,
                    query,
                    bodyPreview);
                return Array.Empty<RawScientificArticle>();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            var root = doc.RootElement;

            if (!root.TryGetProperty("resultList", out var resultList)
                || !resultList.TryGetProperty("result", out var resultArray)
                || resultArray.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning(
                    "Europe PMC response missing resultList.result array. StatusCode={StatusCode}, ContentLength={ContentLength}, ElapsedMs={ElapsedMs}, Query='{Query}'.",
                    statusCode,
                    contentLength,
                    stopwatch.ElapsedMilliseconds,
                    query);
                return Array.Empty<RawScientificArticle>();
            }

            var results = new List<RawScientificArticle>();
            var itemNodeCount = 0;

            foreach (var item in resultArray.EnumerateArray())
            {
                itemNodeCount++;

                // id and source are required for the deduplication key and URL
                if (!item.TryGetProperty("id", out var idProp)
                    || idProp.ValueKind != JsonValueKind.String
                    || string.IsNullOrWhiteSpace(idProp.GetString()))
                    continue;

                if (!item.TryGetProperty("source", out var sourceProp)
                    || sourceProp.ValueKind != JsonValueKind.String
                    || string.IsNullOrWhiteSpace(sourceProp.GetString()))
                    continue;

                var id = idProp.GetString()!;
                var source = sourceProp.GetString()!;

                // title is required
                if (!item.TryGetProperty("title", out var titleProp)
                    || titleProp.ValueKind != JsonValueKind.String
                    || string.IsNullOrWhiteSpace(titleProp.GetString()))
                    continue;

                var title = titleProp.GetString()!;

                // Compose deduplication key and article URL
                var externalGuid = $"{source}:{id}";
                var link = $"https://europepmc.org/article/{source}/{id}";

                // publishedAt — ISO date string, fallback to UtcNow
                DateTime publishedAt;
                if (item.TryGetProperty("firstPublicationDate", out var dateProp)
                    && dateProp.ValueKind == JsonValueKind.String
                    && !string.IsNullOrWhiteSpace(dateProp.GetString())
                    && DateTime.TryParse(
                        dateProp.GetString(),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var parsedDate))
                {
                    publishedAt = parsedDate;
                }
                else
                {
                    publishedAt = DateTime.UtcNow;
                }

                // abstractText may contain HTML fragments like <h4>Background</h4>
                string? description = null;
                if (item.TryGetProperty("abstractText", out var abstractProp)
                    && abstractProp.ValueKind == JsonValueKind.String)
                {
                    description = StripHtml(abstractProp.GetString());
                }

                // journalInfo.journal.title — nested navigation
                string? sourceName = null;
                if (item.TryGetProperty("journalInfo", out var journalInfo)
                    && journalInfo.ValueKind == JsonValueKind.Object
                    && journalInfo.TryGetProperty("journal", out var journal)
                    && journal.ValueKind == JsonValueKind.Object
                    && journal.TryGetProperty("title", out var journalTitle)
                    && journalTitle.ValueKind == JsonValueKind.String)
                {
                    var jt = journalTitle.GetString();
                    sourceName = string.IsNullOrWhiteSpace(jt) ? null : jt;
                }

                // authorString
                string? authors = null;
                if (item.TryGetProperty("authorString", out var authorsProp)
                    && authorsProp.ValueKind == JsonValueKind.String)
                {
                    var a = authorsProp.GetString();
                    authors = string.IsNullOrWhiteSpace(a) ? null : a;
                }

                results.Add(new RawScientificArticle(title, link, publishedAt, description, sourceName, authors, externalGuid));
            }

            _logger.LogInformation(
                "Europe PMC request succeeded. StatusCode={StatusCode}, ContentLength={ContentLength}, ElapsedMs={ElapsedMs}, Query='{Query}', ItemNodes={ItemNodes}, Results={Results}.",
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
                "Europe PMC request timed out after {ElapsedMs} ms for query '{Query}'.",
                stopwatch.ElapsedMilliseconds,
                query);
            return Array.Empty<RawScientificArticle>();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "Europe PMC request canceled by caller after {ElapsedMs} ms for query '{Query}'.",
                stopwatch.ElapsedMilliseconds,
                query);
            return Array.Empty<RawScientificArticle>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "Europe PMC HTTP request failed after {ElapsedMs} ms for query '{Query}'. InnerStatus={InnerStatus}.",
                stopwatch.ElapsedMilliseconds,
                query,
                ex.StatusCode);
            return Array.Empty<RawScientificArticle>();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "Europe PMC returned malformed JSON after {ElapsedMs} ms for query '{Query}'.",
                stopwatch.ElapsedMilliseconds,
                query);
            return Array.Empty<RawScientificArticle>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Europe PMC unexpected failure after {ElapsedMs} ms for query '{Query}'.",
                stopwatch.ElapsedMilliseconds,
                query);
            return Array.Empty<RawScientificArticle>();
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

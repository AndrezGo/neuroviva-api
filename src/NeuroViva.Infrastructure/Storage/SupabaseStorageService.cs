using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NeuroViva.Application.Common.Abstractions;

namespace NeuroViva.Infrastructure.Storage;

public sealed class SupabaseStorageService : IStorageService
{
    private readonly HttpClient _http;
    private readonly SupabaseStorageOptions _options;

    public SupabaseStorageService(HttpClient http, IOptions<SupabaseStorageOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(
        string bucket,
        string path,
        Stream content,
        string contentType,
        CancellationToken ct = default)
    {
        var encodedPath = EncodePath(path);
        var url = $"{_options.Url.TrimEnd('/')}/storage/v1/object/{bucket}/{encodedPath}";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        AddAuthHeaders(requestMessage);
        requestMessage.Headers.Add("x-upsert", "true");

        var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        requestMessage.Content = streamContent;

        var response = await _http.SendAsync(requestMessage, ct);
        await EnsureSuccessAsync(response, ct);

        return path;
    }

    /// <inheritdoc />
    public async Task<string?> GetSignedUrlAsync(
        string bucket,
        string path,
        TimeSpan expiry,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var encodedPath = EncodePath(path);
        var url = $"{_options.Url.TrimEnd('/')}/storage/v1/object/sign/{bucket}/{encodedPath}";

        var expiresIn = (int)expiry.TotalSeconds;
        var body = JsonSerializer.Serialize(new { expiresIn });

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        AddAuthHeaders(requestMessage);
        requestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(requestMessage, ct);
        await EnsureSuccessAsync(response, ct);

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        // Supabase returns: { "signedURL": "/object/sign/{bucket}/{path}?token=..." }
        if (!doc.RootElement.TryGetProperty("signedURL", out var signedUrlElement))
            throw new InvalidOperationException($"Supabase signed URL response did not contain 'signedURL' property. Response: {json}");

        var relativeUrl = signedUrlElement.GetString()
            ?? throw new InvalidOperationException("Supabase signed URL response 'signedURL' property was null.");

        // Build absolute URL: prepend base Supabase URL + /storage/v1
        var baseUrl = _options.Url.TrimEnd('/');
        return $"{baseUrl}/storage/v1{relativeUrl}";
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string bucket,
        string path,
        CancellationToken ct = default)
    {
        var encodedPath = EncodePath(path);
        var url = $"{_options.Url.TrimEnd('/')}/storage/v1/object/{bucket}/{encodedPath}";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
        AddAuthHeaders(requestMessage);

        var response = await _http.SendAsync(requestMessage, ct);
        await EnsureSuccessAsync(response, ct);
    }

    private void AddAuthHeaders(HttpRequestMessage request)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ServiceRoleKey);
        request.Headers.Add("apikey", _options.ServiceRoleKey);
    }

    /// <summary>
    /// URL-encodes each path segment individually (preserving "/" as the separator).
    /// </summary>
    private static string EncodePath(string path) =>
        string.Join("/", path.Split('/').Select(Uri.EscapeDataString));

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException(
            $"Supabase Storage request failed with status {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}",
            inner: null,
            statusCode: response.StatusCode);
    }
}

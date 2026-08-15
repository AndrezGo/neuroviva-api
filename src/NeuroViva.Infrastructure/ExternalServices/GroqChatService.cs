using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Common.Options;

namespace NeuroViva.Infrastructure.ExternalServices;

public sealed class GroqChatService : IGroqChatService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly IOptions<GroqOptions> _options;
    private readonly ILogger<GroqChatService> _logger;

    public GroqChatService(
        HttpClient http,
        IOptions<GroqOptions> options,
        ILogger<GroqChatService> logger)
    {
        _http = http;
        _options = options;
        _logger = logger;
    }

    public async Task<Result<string>> CompleteAsync(
        IReadOnlyList<GroqChatMessage> messages,
        CancellationToken ct)
    {
        var opts = _options.Value;
        var stopwatch = Stopwatch.StartNew();

        var requestBody = new GroqRequestBody(
            Model: opts.Model,
            Messages: messages.Select(m => new GroqMessageDto(m.Role, m.Content)).ToArray(),
            MaxTokens: opts.MaxTokens);

        var json = JsonSerializer.Serialize(requestBody, JsonOptions);

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        // Authorization set per-request (not on the shared HttpClient) to avoid leaking the key.
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", opts.ApiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation(
            "Groq chat/completions request starting. Model='{Model}', MessageCount={MessageCount}.",
            opts.Model,
            messages.Count);

        try
        {
            using var response = await _http.SendAsync(request, ct);
            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                string bodyPreview;
                try
                {
                    var body = await response.Content.ReadAsStringAsync(ct);
                    bodyPreview = body[..Math.Min(500, body.Length)];
                }
                catch
                {
                    bodyPreview = "could not read body preview";
                }

                _logger.LogWarning(
                    "Groq returned non-success status {StatusCode} after {ElapsedMs} ms. BodyPreview='{BodyPreview}'.",
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    bodyPreview);

                return Error.Failure($"ai.http_{statusCode}", $"Groq returned HTTP {statusCode}.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            var root = doc.RootElement;

            if (!root.TryGetProperty("choices", out var choices)
                || choices.ValueKind != JsonValueKind.Array
                || choices.GetArrayLength() == 0)
            {
                _logger.LogWarning(
                    "Groq response missing 'choices' array after {ElapsedMs} ms.",
                    stopwatch.ElapsedMilliseconds);
                return Error.Failure("ai.empty_response", "Groq returned no content.");
            }

            var firstChoice = choices[0];
            if (!firstChoice.TryGetProperty("message", out var message)
                || !message.TryGetProperty("content", out var contentProp)
                || contentProp.ValueKind != JsonValueKind.String)
            {
                _logger.LogWarning(
                    "Groq response missing choices[0].message.content after {ElapsedMs} ms.",
                    stopwatch.ElapsedMilliseconds);
                return Error.Failure("ai.empty_response", "Groq returned no content.");
            }

            var content = contentProp.GetString();
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning(
                    "Groq returned empty content string after {ElapsedMs} ms.",
                    stopwatch.ElapsedMilliseconds);
                return Error.Failure("ai.empty_response", "Groq returned no content.");
            }

            _logger.LogInformation(
                "Groq chat/completions succeeded. StatusCode={StatusCode}, ElapsedMs={ElapsedMs}, ContentLength={ContentLength}.",
                statusCode,
                stopwatch.ElapsedMilliseconds,
                content.Length);

            return content;
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(
                ex,
                "Groq request timed out after {ElapsedMs} ms.",
                stopwatch.ElapsedMilliseconds);
            return Error.Failure("ai.timeout", "The request to the AI assistant timed out. Please try again.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Groq unexpected failure after {ElapsedMs} ms.",
                stopwatch.ElapsedMilliseconds);
            return Error.Failure("ai.unexpected", ex.Message);
        }
    }

    // Private DTOs for Groq/OpenAI request format (snake_case via JsonOptions)
    private sealed record GroqRequestBody(string Model, GroqMessageDto[] Messages, int MaxTokens);
    private sealed record GroqMessageDto(string Role, string Content);
}

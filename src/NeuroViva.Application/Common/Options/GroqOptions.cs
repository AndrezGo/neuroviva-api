namespace NeuroViva.Application.Common.Options;

public sealed class GroqOptions
{
    public const string SectionName = "Groq";

    public string ApiKey { get; set; } = default!;
    public string Model { get; set; } = "openai/gpt-oss-120b";
    public string? ReasoningEffort { get; set; } = "low";
    public int MaxTokens { get; set; } = 1536;
    public int TimeoutSeconds { get; set; } = 60;
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/";
}

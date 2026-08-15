namespace NeuroViva.Application.Common.Options;

public sealed class GroqOptions
{
    public const string SectionName = "Groq";

    public string ApiKey { get; set; } = default!;
    public string Model { get; set; } = "llama-3.3-70b-versatile";
    public int MaxTokens { get; set; } = 1024;
    public int TimeoutSeconds { get; set; } = 60;
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/";
}

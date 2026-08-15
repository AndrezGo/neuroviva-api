using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Common.Abstractions;

public interface IGroqChatService
{
    Task<Result<string>> CompleteAsync(IReadOnlyList<GroqChatMessage> messages, CancellationToken ct);
}

/// <summary>Role is one of: "system", "user", "assistant".</summary>
public sealed record GroqChatMessage(string Role, string Content);

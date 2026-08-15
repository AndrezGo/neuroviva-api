namespace NeuroViva.Application.Ai.Queries;

/// <summary>Role is serialized as lowercase "user" or "assistant".</summary>
public sealed record ChatMessageDto(
    Guid Id,
    string Role,
    string Content,
    DateTime CreatedAt);

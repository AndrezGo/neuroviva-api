using NeuroViva.Domain.Ai.Enums;
using NeuroViva.Domain.Common;
using NeuroViva.Domain.Exceptions;

namespace NeuroViva.Domain.Ai;

public sealed class AiChatMessage : Entity<Guid>
{
    public Guid ConversationId { get; private set; }
    public AiChatRole Role { get; private set; }
    public string Content { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private AiChatMessage() { }

    public static AiChatMessage Create(Guid conversationId, AiChatRole role, string content, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new BusinessRuleViolationException(
                "chat_message.content_required",
                "Message content cannot be empty.");

        return new AiChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = role,
            Content = content,
            CreatedAt = now
        };
    }
}

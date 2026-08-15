namespace NeuroViva.Domain.Ai.Repositories;

public interface IAiChatMessageRepository
{
    Task<IReadOnlyList<AiChatMessage>> ListByConversationOrderedAsync(Guid conversationId, CancellationToken ct = default);
    Task AddAsync(AiChatMessage message, CancellationToken ct = default);
}

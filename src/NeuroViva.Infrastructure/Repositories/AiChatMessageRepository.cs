using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class AiChatMessageRepository : IAiChatMessageRepository
{
    private readonly NeuroVivaDbContext _db;

    public AiChatMessageRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<IReadOnlyList<AiChatMessage>> ListByConversationOrderedAsync(
        Guid conversationId,
        CancellationToken ct = default)
        => await _db.AiChatMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(AiChatMessage message, CancellationToken ct = default)
        => await _db.AiChatMessages.AddAsync(message, ct);
}

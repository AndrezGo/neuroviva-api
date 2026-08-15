using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class AiChatConversationRepository : IAiChatConversationRepository
{
    private readonly NeuroVivaDbContext _db;

    public AiChatConversationRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<AiChatConversation?> GetActiveByDoctorAndPatientAsync(
        Guid doctorId,
        Guid patientId,
        CancellationToken ct = default)
        => await _db.AiChatConversations
            .FirstOrDefaultAsync(c => c.DoctorId == doctorId && c.PatientId == patientId, ct);

    public async Task AddAsync(AiChatConversation conversation, CancellationToken ct = default)
        => await _db.AiChatConversations.AddAsync(conversation, ct);

    public void Update(AiChatConversation conversation)
        => _db.AiChatConversations.Update(conversation);
}

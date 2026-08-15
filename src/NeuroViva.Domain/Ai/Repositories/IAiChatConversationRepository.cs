namespace NeuroViva.Domain.Ai.Repositories;

public interface IAiChatConversationRepository
{
    Task<AiChatConversation?> GetActiveByDoctorAndPatientAsync(Guid doctorId, Guid patientId, CancellationToken ct = default);
    Task AddAsync(AiChatConversation conversation, CancellationToken ct = default);
    void Update(AiChatConversation conversation);
}

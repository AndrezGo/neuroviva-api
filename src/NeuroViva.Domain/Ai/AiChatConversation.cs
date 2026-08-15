using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Ai;

public sealed class AiChatConversation : AggregateRoot<Guid>
{
    public Guid DoctorId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastMessageAt { get; private set; }

    private AiChatConversation() { }

    public static AiChatConversation Create(Guid doctorId, Guid patientId, Guid tenantId, DateTime now)
    {
        return new AiChatConversation
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            PatientId = patientId,
            TenantId = tenantId,
            CreatedAt = now,
            LastMessageAt = null
        };
    }

    public void TouchLastMessage(DateTime now) => LastMessageAt = now;
}

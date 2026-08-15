using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Ai;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class AiChatConversationConfiguration : IEntityTypeConfiguration<AiChatConversation>
{
    public void Configure(EntityTypeBuilder<AiChatConversation> builder)
    {
        builder.ToTable("ai_chat_conversation");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.DoctorId).HasColumnName("doctor_id");
        builder.Property(c => c.PatientId).HasColumnName("patient_id");
        builder.Property(c => c.TenantId).HasColumnName("tenant_id");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.LastMessageAt).HasColumnName("last_message_at");
        builder.Ignore(c => c.DomainEvents);
    }
}

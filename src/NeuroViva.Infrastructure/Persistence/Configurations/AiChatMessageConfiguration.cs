using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Ai;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class AiChatMessageConfiguration : IEntityTypeConfiguration<AiChatMessage>
{
    public void Configure(EntityTypeBuilder<AiChatMessage> builder)
    {
        builder.ToTable("ai_chat_message");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.ConversationId).HasColumnName("conversation_id");
        builder.Property(m => m.Role)
            .HasColumnName("role")
            .HasConversion<short>();
        builder.Property(m => m.Content)
            .HasColumnName("content")
            .HasColumnType("text");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Ignore(m => m.DomainEvents);

        builder.HasOne<AiChatConversation>()
            .WithMany()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

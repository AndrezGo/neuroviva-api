using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Ai;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notification");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.UserId).HasColumnName("user_id");
        builder.Property(n => n.AlertId).HasColumnName("alert_id");
        builder.Property(n => n.Channel).HasColumnName("channel");
        builder.Property(n => n.Status).HasColumnName("status");
        builder.Property(n => n.SentAt).HasColumnName("sent_at");
        builder.Property(n => n.CreatedAt).HasColumnName("created_at");
    }
}

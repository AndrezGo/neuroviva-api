using NeuroViva.Domain.Ai.Enums;

namespace NeuroViva.Application.Common.Abstractions;

public interface INotificationDispatcher
{
    Task SendAsync(
        Guid userId,
        NotificationChannel channel,
        string title,
        string body,
        CancellationToken ct = default);
}

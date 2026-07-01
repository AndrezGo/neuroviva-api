namespace NeuroViva.Application.Caregivers.Queries.GetNotifications;

public record NotificationDto(Guid Id, string Title, string Body, bool IsRead, DateTime CreatedAt);

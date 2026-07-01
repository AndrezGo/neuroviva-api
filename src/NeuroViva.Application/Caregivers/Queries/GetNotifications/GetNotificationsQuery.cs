using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetNotifications;

public record GetNotificationsQuery() : IRequest<Result<NotificationDto[]>>;

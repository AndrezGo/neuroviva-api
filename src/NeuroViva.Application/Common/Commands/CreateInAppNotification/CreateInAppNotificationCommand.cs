using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Common.Commands.CreateInAppNotification;

public record CreateInAppNotificationCommand(Guid UserId, string Title, string Body) : IRequest<Result>;

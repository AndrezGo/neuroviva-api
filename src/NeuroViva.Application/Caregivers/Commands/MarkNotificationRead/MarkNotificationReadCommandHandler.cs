using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Ai.Repositories;

namespace NeuroViva.Application.Caregivers.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler
    : IRequestHandler<MarkNotificationReadCommand, Result>
{
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationRepository _repo;

    public MarkNotificationReadCommandHandler(
        ICurrentUserService currentUser,
        INotificationRepository repo)
    {
        _currentUser = currentUser;
        _repo = repo;
    }

    public async Task<Result> Handle(
        MarkNotificationReadCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var currentUserId = _currentUser.UserId.Value;

        var notification = await _repo.FindByIdAsync(request.NotificationId, cancellationToken);
        if (notification is null)
            return Error.NotFound("notification.not_found", "Notification not found.");

        if (notification.UserId != currentUserId)
            return Error.Forbidden("You do not have permission to access this notification.");

        notification.MarkRead();
        await _repo.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}

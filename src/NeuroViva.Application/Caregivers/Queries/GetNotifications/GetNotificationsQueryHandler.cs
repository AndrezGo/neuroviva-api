using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Ai.Repositories;

namespace NeuroViva.Application.Caregivers.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, Result<NotificationDto[]>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationRepository _repo;

    public GetNotificationsQueryHandler(
        ICurrentUserService currentUser,
        INotificationRepository repo)
    {
        _currentUser = currentUser;
        _repo = repo;
    }

    public async Task<Result<NotificationDto[]>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var userId = _currentUser.UserId.Value;

        var notifications = await _repo.ListInAppAsync(userId, 30, cancellationToken);

        var dtos = notifications
            .Select(n => new NotificationDto(
                n.Id,
                n.Title,
                n.Body,
                n.ReadAt is not null,
                n.CreatedAt))
            .ToArray();

        return Result<NotificationDto[]>.Success(dtos);
    }
}

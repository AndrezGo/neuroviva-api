using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Repositories;

namespace NeuroViva.Application.Common.Commands.CreateInAppNotification;

public sealed class CreateInAppNotificationCommandHandler
    : IRequestHandler<CreateInAppNotificationCommand, Result>
{
    private readonly INotificationRepository _repo;

    public CreateInAppNotificationCommandHandler(INotificationRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(
        CreateInAppNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = Notification.Create(request.UserId, "inapp", request.Title, request.Body);

        await _repo.AddAsync(notification, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}

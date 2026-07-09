using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Community.Repositories;

namespace NeuroViva.Application.Community.Commands.RemoveReaction;

public sealed class RemoveReactionCommandHandler
    : IRequestHandler<RemoveReactionCommand, Result<RemoveReactionResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICommunityReactionRepository _reactionRepo;
    private readonly IUnitOfWork _uow;

    public RemoveReactionCommandHandler(
        ICurrentUserService currentUser,
        ICommunityReactionRepository reactionRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _reactionRepo = reactionRepo;
        _uow = uow;
    }

    public async Task<Result<RemoveReactionResult>> Handle(
        RemoveReactionCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var userId = _currentUser.UserId.Value;

        var reaction = await _reactionRepo.GetAsync(request.PostId, userId, request.Type, cancellationToken);
        if (reaction is null)
            return new RemoveReactionResult();

        _reactionRepo.Remove(reaction);
        await _uow.SaveChangesAsync(cancellationToken);

        return new RemoveReactionResult();
    }
}

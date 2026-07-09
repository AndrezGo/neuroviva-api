using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Community;
using NeuroViva.Domain.Community.Repositories;

namespace NeuroViva.Application.Community.Commands.AddReaction;

public sealed class AddReactionCommandHandler
    : IRequestHandler<AddReactionCommand, Result<AddReactionResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICommunityPostRepository _postRepo;
    private readonly ICommunityReactionRepository _reactionRepo;
    private readonly IUnitOfWork _uow;

    public AddReactionCommandHandler(
        ICurrentUserService currentUser,
        ICommunityPostRepository postRepo,
        ICommunityReactionRepository reactionRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _postRepo = postRepo;
        _reactionRepo = reactionRepo;
        _uow = uow;
    }

    public async Task<Result<AddReactionResult>> Handle(
        AddReactionCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var userId = _currentUser.UserId.Value;

        var post = await _postRepo.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null || post.Removed)
            return Error.NotFound("post.not_found", "Post not found.");

        var existing = await _reactionRepo.GetAsync(request.PostId, userId, request.Type, cancellationToken);
        if (existing is not null)
            return new AddReactionResult(existing.Id);

        var reaction = CommunityReaction.Add(request.PostId, userId, request.Type);
        await _reactionRepo.AddAsync(reaction, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new AddReactionResult(reaction.Id);
    }
}

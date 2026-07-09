using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Community.Repositories;

namespace NeuroViva.Application.Community.Commands.ModeratePost;

public sealed class ModeratePostCommandHandler
    : IRequestHandler<ModeratePostCommand, Result<ModeratePostResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICommunityPostRepository _postRepo;
    private readonly IUnitOfWork _uow;

    public ModeratePostCommandHandler(
        ICurrentUserService currentUser,
        ICommunityPostRepository postRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _postRepo = postRepo;
        _uow = uow;
    }

    public async Task<Result<ModeratePostResult>> Handle(
        ModeratePostCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var post = await _postRepo.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Error.NotFound("post.not_found", "Post not found.");

        if (post.Removed)
            return new ModeratePostResult();

        post.Moderate(request.Reason);
        _postRepo.Update(post);
        await _uow.SaveChangesAsync(cancellationToken);

        return new ModeratePostResult();
    }
}

using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Community;
using NeuroViva.Domain.Community.Repositories;

namespace NeuroViva.Application.Community.Commands.CreateComment;

public sealed class CreateCommentCommandHandler
    : IRequestHandler<CreateCommentCommand, Result<CreateCommentResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICommunityPostRepository _postRepo;
    private readonly IGroupRepository _groupRepo;
    private readonly IGroupMemberRepository _groupMemberRepo;
    private readonly ICommunityCommentRepository _commentRepo;
    private readonly IUnitOfWork _uow;

    public CreateCommentCommandHandler(
        ICurrentUserService currentUser,
        ICommunityPostRepository postRepo,
        IGroupRepository groupRepo,
        IGroupMemberRepository groupMemberRepo,
        ICommunityCommentRepository commentRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _postRepo = postRepo;
        _groupRepo = groupRepo;
        _groupMemberRepo = groupMemberRepo;
        _commentRepo = commentRepo;
        _uow = uow;
    }

    public async Task<Result<CreateCommentResult>> Handle(
        CreateCommentCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var userId = _currentUser.UserId.Value;

        var post = await _postRepo.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null || post.Removed)
            return Error.NotFound("post.not_found", "Post not found.");

        if (post.DiseaseId is null)
            return Error.Forbidden("This post is not associated with a group.");

        var groups = await _groupRepo.ListActiveByDiseaseIdsAsync(
            new[] { post.DiseaseId.Value },
            cancellationToken);

        var group = groups.FirstOrDefault();
        if (group is null)
            return Error.NotFound("group.not_found", "No active group found for this post.");

        var isMember = await _groupMemberRepo.IsActiveMemberAsync(group.Id, userId, cancellationToken);
        if (!isMember)
            return Error.Forbidden("You are not a member of this group.");

        var comment = CommunityComment.Create(
            postId: request.PostId,
            authorId: userId,
            content: request.Content);

        await _commentRepo.AddAsync(comment, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new CreateCommentResult(comment.Id);
    }
}

using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Community.Repositories;

namespace NeuroViva.Application.Community.Queries.GetPostComments;

public sealed class GetPostCommentsQueryHandler
    : IRequestHandler<GetPostCommentsQuery, Result<IReadOnlyList<CommentFeedItemDto>>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICommunityPostRepository _postRepo;
    private readonly IGroupRepository _groupRepo;
    private readonly IGroupMemberRepository _groupMemberRepo;
    private readonly ICommunityCommentRepository _commentRepo;

    public GetPostCommentsQueryHandler(
        ICurrentUserService currentUser,
        ICommunityPostRepository postRepo,
        IGroupRepository groupRepo,
        IGroupMemberRepository groupMemberRepo,
        ICommunityCommentRepository commentRepo)
    {
        _currentUser = currentUser;
        _postRepo = postRepo;
        _groupRepo = groupRepo;
        _groupMemberRepo = groupMemberRepo;
        _commentRepo = commentRepo;
    }

    public async Task<Result<IReadOnlyList<CommentFeedItemDto>>> Handle(
        GetPostCommentsQuery request,
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

        var comments = await _commentRepo.ListByPostPagedAsync(
            request.PostId,
            request.Skip,
            request.Take,
            cancellationToken);

        var dtos = comments.Select(c => new CommentFeedItemDto(
            Id: c.Id,
            PostId: c.PostId,
            AuthorId: c.AuthorId,
            Content: c.Removed ? "[Contenido retirado por moderación]" : c.Content,
            CreatedAt: c.CreatedAt,
            Removed: c.Removed,
            RemovedReason: c.RemovedReason
        )).ToList();

        return Result<IReadOnlyList<CommentFeedItemDto>>.Success(dtos);
    }
}

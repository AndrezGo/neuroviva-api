using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Community.Repositories;

namespace NeuroViva.Application.Community.Queries.GetGroupFeed;

public sealed class GetGroupFeedQueryHandler
    : IRequestHandler<GetGroupFeedQuery, Result<IReadOnlyList<PostFeedItemDto>>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IGroupRepository _groupRepo;
    private readonly IGroupMemberRepository _groupMemberRepo;
    private readonly ICommunityPostRepository _postRepo;
    private readonly ICommunityReactionRepository _reactionRepo;

    public GetGroupFeedQueryHandler(
        ICurrentUserService currentUser,
        IGroupRepository groupRepo,
        IGroupMemberRepository groupMemberRepo,
        ICommunityPostRepository postRepo,
        ICommunityReactionRepository reactionRepo)
    {
        _currentUser = currentUser;
        _groupRepo = groupRepo;
        _groupMemberRepo = groupMemberRepo;
        _postRepo = postRepo;
        _reactionRepo = reactionRepo;
    }

    public async Task<Result<IReadOnlyList<PostFeedItemDto>>> Handle(
        GetGroupFeedQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var userId = _currentUser.UserId.Value;

        var group = await _groupRepo.GetByIdAsync(request.GroupId, cancellationToken);
        if (group is null || !group.Active)
            return Error.NotFound("group.not_found", "Group not found.");

        var isMember = await _groupMemberRepo.IsActiveMemberAsync(request.GroupId, userId, cancellationToken);
        if (!isMember)
            return Error.Forbidden("You are not a member of this group.");

        var posts = await _postRepo.ListByGroupAsync(
            request.GroupId,
            request.Skip,
            request.Take,
            cancellationToken);

        var postIds = posts.Select(p => p.Id).ToList();

        var reactionsByPost = postIds.Count > 0
            ? await _reactionRepo.CountByPostsAsync(postIds, cancellationToken)
            : new Dictionary<Guid, IReadOnlyDictionary<string, int>>();

        var dtos = posts.Select(p => new PostFeedItemDto(
            Id: p.Id,
            AuthorId: p.AuthorId,
            Content: p.Removed ? "[Contenido retirado por moderación]" : p.Content,
            CreatedAt: p.CreatedAt,
            Removed: p.Removed,
            RemovedReason: p.RemovedReason,
            Reactions: reactionsByPost.TryGetValue(p.Id, out var r)
                ? r
                : new Dictionary<string, int>()
        )).ToList();

        return Result<IReadOnlyList<PostFeedItemDto>>.Success(dtos);
    }
}

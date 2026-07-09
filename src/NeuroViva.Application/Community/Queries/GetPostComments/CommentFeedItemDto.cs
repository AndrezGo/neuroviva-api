namespace NeuroViva.Application.Community.Queries.GetPostComments;

public sealed record CommentFeedItemDto(
    Guid Id,
    Guid PostId,
    Guid AuthorId,
    string Content,
    DateTime CreatedAt,
    bool Removed,
    string? RemovedReason);

namespace NeuroViva.Application.Community.Queries.GetGroupFeed;

public sealed record PostFeedItemDto(
    Guid Id,
    Guid AuthorId,
    string Content,
    DateTime CreatedAt,
    bool Removed,
    string? RemovedReason,
    IReadOnlyDictionary<string, int> Reactions,
    IReadOnlyList<string> MyReactions,
    int CommentCount);

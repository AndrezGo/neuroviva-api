namespace NeuroViva.Domain.Community.Repositories;

public interface ICommunityReactionRepository
{
    Task<CommunityReaction?> GetAsync(Guid postId, Guid userId, string type, CancellationToken ct = default);
    Task<IReadOnlyDictionary<string, int>> CountByPostAsync(Guid postId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, int>>> CountByPostsAsync(IReadOnlyCollection<Guid> postIds, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<string>>> ListUserReactionTypesByPostsAsync(
        IReadOnlyCollection<Guid> postIds,
        Guid userId,
        CancellationToken ct = default);
    Task AddAsync(CommunityReaction reaction, CancellationToken ct = default);
    void Remove(CommunityReaction reaction);
}

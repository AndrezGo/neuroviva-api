namespace NeuroViva.Domain.Community.Repositories;

public interface ICommunityPostRepository
{
    Task<CommunityPost?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CommunityPost>> ListByGroupAsync(Guid groupId, int skip, int take, CancellationToken ct = default);
    Task AddAsync(CommunityPost post, CancellationToken ct = default);
    void Update(CommunityPost post);
}

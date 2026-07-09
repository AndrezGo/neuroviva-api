namespace NeuroViva.Domain.Community.Repositories;

public interface ICommunityCommentRepository
{
    Task<CommunityComment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CommunityComment>> ListByPostAsync(Guid postId, CancellationToken ct = default);
    Task AddAsync(CommunityComment comment, CancellationToken ct = default);
    void Update(CommunityComment comment);
}

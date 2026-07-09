using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Community;
using NeuroViva.Domain.Community.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class CommunityCommentRepository : ICommunityCommentRepository
{
    private readonly NeuroVivaDbContext _db;

    public CommunityCommentRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<CommunityComment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.CommunityComments.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CommunityComment>> ListByPostAsync(Guid postId, CancellationToken ct = default)
        => await _db.CommunityComments
            .AsNoTracking()
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CommunityComment>> ListByPostPagedAsync(
        Guid postId, int skip, int take, CancellationToken ct = default)
        => await _db.CommunityComments
            .AsNoTracking()
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

    public async Task<IReadOnlyDictionary<Guid, int>> CountByPostsAsync(
        IReadOnlyCollection<Guid> postIds,
        CancellationToken ct = default)
    {
        var counts = await _db.CommunityComments
            .AsNoTracking()
            .Where(c => postIds.Contains(c.PostId))
            .GroupBy(c => c.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return counts.ToDictionary(x => x.PostId, x => x.Count);
    }

    public async Task AddAsync(CommunityComment comment, CancellationToken ct = default)
        => await _db.CommunityComments.AddAsync(comment, ct);

    public void Update(CommunityComment comment)
        => _db.CommunityComments.Update(comment);
}

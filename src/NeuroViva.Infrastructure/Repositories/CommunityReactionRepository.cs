using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Community;
using NeuroViva.Domain.Community.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class CommunityReactionRepository : ICommunityReactionRepository
{
    private readonly NeuroVivaDbContext _db;

    public CommunityReactionRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<CommunityReaction?> GetAsync(Guid postId, Guid userId, string type, CancellationToken ct = default)
        => await _db.CommunityReactions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId && r.Type == type, ct);

    public async Task<IReadOnlyDictionary<string, int>> CountByPostAsync(Guid postId, CancellationToken ct = default)
    {
        var counts = await _db.CommunityReactions
            .AsNoTracking()
            .Where(r => r.PostId == postId)
            .GroupBy(r => r.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return counts.ToDictionary(x => x.Type, x => x.Count);
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, int>>> CountByPostsAsync(
        IReadOnlyCollection<Guid> postIds,
        CancellationToken ct = default)
    {
        var rows = await _db.CommunityReactions
            .AsNoTracking()
            .Where(r => postIds.Contains(r.PostId))
            .GroupBy(r => new { r.PostId, r.Type })
            .Select(g => new { g.Key.PostId, g.Key.Type, Count = g.Count() })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => x.PostId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<string, int>)g.ToDictionary(x => x.Type, x => x.Count));
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<string>>> ListUserReactionTypesByPostsAsync(
        IReadOnlyCollection<Guid> postIds,
        Guid userId,
        CancellationToken ct = default)
    {
        var rows = await _db.CommunityReactions
            .AsNoTracking()
            .Where(r => postIds.Contains(r.PostId) && r.UserId == userId)
            .Select(r => new { r.PostId, r.Type })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => x.PostId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g.Select(x => x.Type).ToList());
    }

    public async Task AddAsync(CommunityReaction reaction, CancellationToken ct = default)
        => await _db.CommunityReactions.AddAsync(reaction, ct);

    public void Remove(CommunityReaction reaction)
        => _db.CommunityReactions.Remove(reaction);
}

using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Community;
using NeuroViva.Domain.Community.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class CommunityPostRepository : ICommunityPostRepository
{
    private readonly NeuroVivaDbContext _db;

    public CommunityPostRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<CommunityPost?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.CommunityPosts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<CommunityPost>> ListByGroupAsync(
        Guid groupId,
        int skip,
        int take,
        CancellationToken ct = default)
    {
        var group = await _db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == groupId, ct);

        if (group?.DiseaseId is null)
            return Array.Empty<CommunityPost>();

        return await _db.CommunityPosts
            .AsNoTracking()
            .Where(p => p.DiseaseId == group.DiseaseId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task AddAsync(CommunityPost post, CancellationToken ct = default)
        => await _db.CommunityPosts.AddAsync(post, ct);

    public void Update(CommunityPost post)
        => _db.CommunityPosts.Update(post);
}

using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Community;
using NeuroViva.Domain.Community.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class GroupRepository : IGroupRepository
{
    private readonly NeuroVivaDbContext _db;

    public GroupRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<Group?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await _db.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Slug == slug, ct);

    public async Task<IReadOnlyList<Group>> ListActiveByDiseaseIdsAsync(
        IReadOnlyCollection<Guid> diseaseIds,
        CancellationToken ct = default)
        => await _db.Groups
            .AsNoTracking()
            .Where(g => g.Active && g.DiseaseId != null && diseaseIds.Contains(g.DiseaseId.Value))
            .ToListAsync(ct);

    public async Task AddAsync(Group group, CancellationToken ct = default)
        => await _db.Groups.AddAsync(group, ct);
}

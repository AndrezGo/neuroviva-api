using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Content;
using NeuroViva.Domain.Content.Enums;
using NeuroViva.Domain.Content.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class ResourceRepository : IResourceRepository
{
    private readonly NeuroVivaDbContext _db;

    public ResourceRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Resource>> ListApprovedAsync(
        ResourceType type,
        IReadOnlyCollection<Guid> diseaseIds,
        CancellationToken ct = default)
        => await _db.Resources
            .AsNoTracking()
            .Where(r => r.Type == type
                && r.ApprovalStatus == "aprobado"
                && (r.DiseaseId == null || diseaseIds.Contains(r.DiseaseId.Value)))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Resource>> ListPendingAsync(CancellationToken ct = default)
        => await _db.Resources
            .AsNoTracking()
            .Where(r => r.ApprovalStatus == "pendiente")
            .ToListAsync(ct);

    public async Task AddAsync(Resource resource, CancellationToken ct = default)
        => await _db.Resources.AddAsync(resource, ct);

    public void Update(Resource resource)
        => _db.Resources.Update(resource);
}

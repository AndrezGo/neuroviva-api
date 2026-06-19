using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Catalog;
using NeuroViva.Domain.Catalog.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class DiseaseRepository : IDiseaseRepository
{
    private readonly NeuroVivaDbContext _db;

    public DiseaseRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Disease?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Diseases.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<Disease?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await _db.Diseases.FirstOrDefaultAsync(d => d.Slug == slug, ct);

    public async Task<Disease?> GetByNameAsync(string name, CancellationToken ct = default)
        => await _db.Diseases.FirstOrDefaultAsync(
            d => d.Name.ToLower() == name.ToLower(), ct);

    public async Task<IReadOnlyList<Disease>> ListActiveAsync(CancellationToken ct = default)
        => await _db.Diseases.Where(d => d.IsActive).ToListAsync(ct);
}

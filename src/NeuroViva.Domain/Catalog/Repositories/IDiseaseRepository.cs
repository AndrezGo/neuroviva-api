namespace NeuroViva.Domain.Catalog.Repositories;

public interface IDiseaseRepository
{
    Task<Disease?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Disease?> GetBySlugAsync(string slug, CancellationToken ct = default);
    /// <summary>Case-insensitive lookup by name. Used as fallback when slug lookup returns null.</summary>
    Task<Disease?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Disease>> ListActiveAsync(CancellationToken ct = default);
}

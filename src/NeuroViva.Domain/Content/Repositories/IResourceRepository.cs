using NeuroViva.Domain.Content.Enums;

namespace NeuroViva.Domain.Content.Repositories;

public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Resource>> ListApprovedAsync(ResourceType type, IReadOnlyCollection<Guid> diseaseIds, CancellationToken ct = default);
    Task<IReadOnlyList<Resource>> ListPendingAsync(CancellationToken ct = default);
    Task AddAsync(Resource resource, CancellationToken ct = default);
    void Update(Resource resource);
}

namespace NeuroViva.Domain.Community.Repositories;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Group?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Group>> ListActiveByDiseaseIdsAsync(IReadOnlyCollection<Guid> diseaseIds, CancellationToken ct = default);
    Task AddAsync(Group group, CancellationToken ct = default);
}

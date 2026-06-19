namespace NeuroViva.Domain.Users.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

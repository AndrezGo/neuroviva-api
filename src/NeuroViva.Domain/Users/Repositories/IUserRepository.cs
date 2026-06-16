namespace NeuroViva.Domain.Users.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByAuthUserIdAsync(Guid authUserId, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
}

namespace NeuroViva.Domain.Users.Repositories;

public interface IUserRoleRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task AddAsync(UserRole userRole, CancellationToken ct = default);
}

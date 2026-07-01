namespace NeuroViva.Domain.Ai.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken ct = default);
    Task<List<Notification>> ListInAppAsync(Guid userId, int limit = 30, CancellationToken ct = default);
    Task<Notification?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

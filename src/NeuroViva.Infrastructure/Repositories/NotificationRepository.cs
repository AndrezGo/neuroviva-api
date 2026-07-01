using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NeuroVivaDbContext _db;

    public NotificationRepository(NeuroVivaDbContext db) => _db = db;

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
        => await _db.Notifications.AddAsync(notification, ct);

    public async Task<List<Notification>> ListInAppAsync(Guid userId, int limit = 30, CancellationToken ct = default)
        => await _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.Channel == "inapp")
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<Notification?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}

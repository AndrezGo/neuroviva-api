using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Enums;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class AlertRepository : IAlertRepository
{
    private readonly NeuroVivaDbContext _db;

    public AlertRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Alerts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Alert>> ListByDoctorAsync(
        Guid doctorId,
        bool includeResolved = false,
        CancellationToken ct = default)
    {
        var query = _db.Alerts
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId);

        if (!includeResolved)
            query = query.Where(a => !a.Resolved);

        var alerts = await query.ToListAsync(ct);

        return alerts
            .OrderByDescending(a => PriorityRank(a.Priority))
            .ThenByDescending(a => a.CreatedAt)
            .ToList();
    }

    public async Task AddAsync(Alert alert, CancellationToken ct = default)
        => await _db.Alerts.AddAsync(alert, ct);

    public void Update(Alert alert)
        => _db.Alerts.Update(alert);

    private static int PriorityRank(AlertPriority priority) => priority switch
    {
        AlertPriority.Critical => 3,
        AlertPriority.High => 2,
        AlertPriority.Medium => 1,
        AlertPriority.Info => 0,
        _ => 0
    };
}

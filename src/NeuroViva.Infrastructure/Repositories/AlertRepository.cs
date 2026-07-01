using Microsoft.EntityFrameworkCore;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Enums;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class AlertRepository : IAlertRepository
{
    private readonly NeuroVivaDbContext _db;
    private readonly IClock _clock;

    public AlertRepository(NeuroVivaDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

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

    public async Task<bool> ExistsRecentAsync(
        Guid patientId, string type, AlertPriority priority, TimeSpan window, CancellationToken ct = default)
    {
        var cutoff = _clock.UtcNow - window;
        return await _db.Alerts.AnyAsync(
            a => a.PatientId == patientId
                 && a.Type == type
                 && a.Priority == priority
                 && !a.Resolved
                 && a.CreatedAt > cutoff,
            ct);
    }

    public async Task<bool> ExistsForSourceAsync(Guid sourceReferenceId, CancellationToken ct = default)
        => await _db.Alerts.AnyAsync(a => a.SourceReferenceId == sourceReferenceId, ct);

    private static int PriorityRank(AlertPriority priority) => priority switch
    {
        AlertPriority.Critical => 3,
        AlertPriority.High => 2,
        AlertPriority.Medium => 1,
        AlertPriority.Info => 0,
        _ => 0
    };
}

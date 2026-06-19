using NeuroViva.Domain.Medications;
using NeuroViva.Domain.Medications.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class MedicationLogRepository : IMedicationLogRepository
{
    private readonly NeuroVivaDbContext _db;

    public MedicationLogRepository(NeuroVivaDbContext db) => _db = db;

    public async Task AddAsync(MedicationLog log, CancellationToken ct = default)
        => await _db.MedicationLogs.AddAsync(log, ct);
}

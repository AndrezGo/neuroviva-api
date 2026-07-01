using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class ClinicalRecordRepository : IClinicalRecordRepository
{
    private readonly NeuroVivaDbContext _db;

    public ClinicalRecordRepository(NeuroVivaDbContext db) => _db = db;

    public async Task AddAsync(ClinicalRecord record, CancellationToken ct = default)
        => await _db.ClinicalRecords.AddAsync(record, ct);
}

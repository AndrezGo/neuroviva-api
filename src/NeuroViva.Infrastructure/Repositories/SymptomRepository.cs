using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.HealthMonitoring;
using NeuroViva.Domain.HealthMonitoring.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class SymptomRepository : ISymptomRepository
{
    private readonly NeuroVivaDbContext _db;

    public SymptomRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Symptom?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Symptoms.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);

    public async Task<IReadOnlyList<Symptom>> ListByPatientAsync(Guid patientId, int limit = 50, CancellationToken ct = default)
        => await _db.Symptoms
            .Where(s => s.PatientId == patientId && !s.IsDeleted)
            .OrderByDescending(s => s.LoggedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task AddAsync(Symptom symptom, CancellationToken ct = default)
        => await _db.Symptoms.AddAsync(symptom, ct);

    public void Update(Symptom symptom) => _db.Symptoms.Update(symptom);
}

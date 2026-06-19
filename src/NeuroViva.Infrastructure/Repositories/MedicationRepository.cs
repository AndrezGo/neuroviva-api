using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Medications;
using NeuroViva.Domain.Medications.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class MedicationRepository : IMedicationRepository
{
    private readonly NeuroVivaDbContext _db;

    public MedicationRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Medication?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Medications.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<Medication>> ListActiveByPatientAsync(Guid patientId, CancellationToken ct = default)
        => await _db.Medications
            .Where(m => m.PatientId == patientId && m.IsActive)
            .ToListAsync(ct);

    public async Task AddAsync(Medication medication, CancellationToken ct = default)
        => await _db.Medications.AddAsync(medication, ct);

    public void Update(Medication medication) => _db.Medications.Update(medication);
}

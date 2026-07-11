using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly NeuroVivaDbContext _db;

    public PatientRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Patients.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _db.Patients.AnyAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Patient>> ListByDoctorAsync(Guid doctorId, CancellationToken ct = default)
        => await _db.Patients
            .Join(
                _db.PatientDoctors.Where(pd => pd.DoctorId == doctorId),
                p => p.Id,
                pd => pd.PatientId,
                (p, pd) => p)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Patient>> ListByCaregiverAsync(Guid caregiverId, CancellationToken ct = default)
        => await _db.Patients
            .Join(
                _db.PatientCaregivers.Where(pc => pc.CaregiverId == caregiverId),
                p => p.Id,
                pc => pc.PatientId,
                (p, pc) => p)
            .ToListAsync(ct);

    public async Task<Patient?> GetByDocumentNumberAsync(
        Guid tenantId,
        string documentNumber,
        CancellationToken ct = default)
    {
        var normalized = documentNumber.Trim().ToUpperInvariant();
        // IgnoreQueryFilters because the global filter requires _tenantContext to be set,
        // which is always true here, but we filter explicitly by tenantId for clarity and safety.
        return await _db.Patients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.DocumentNumber == normalized, ct);
    }

    public async Task<Patient?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        // UserId is globally unique — crosses tenants, so we ignore the global tenant filter.
        return await _db.Patients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);
    }

    public async Task<Patient?> FindClaimableByDocumentNumberAsync(
        string documentNumber,
        Guid preferredUserId,
        CancellationToken ct = default)
    {
        var normalized = documentNumber.Trim().ToUpperInvariant();

        return await _db.Patients
            .IgnoreQueryFilters()
            .Where(p => p.DocumentNumber == normalized)
            .OrderBy(p => p.UserId == preferredUserId ? 0 : (p.UserId == null ? 1 : 2))
            .ThenBy(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(Patient patient, CancellationToken ct = default)
        => await _db.Patients.AddAsync(patient, ct);

    public void Update(Patient patient) => _db.Patients.Update(patient);
}

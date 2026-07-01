using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Enums;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class PatientCaregiverRepository : IPatientCaregiverRepository
{
    private readonly NeuroVivaDbContext _db;

    public PatientCaregiverRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<IReadOnlyList<PatientCaregiverWithPatient>> GetActiveByCaregiverAsync(
        Guid caregiverId,
        CancellationToken ct = default)
    {
        // No global query filter on PatientCaregiver or Patient (when accessed through join).
        // We filter by active status explicitly. IgnoreQueryFilters to be safe against the
        // global tenant filter being applied to Patient when accessed via this navigation.
        var rows = await _db.PatientCaregivers
            .Where(pc => pc.CaregiverId == caregiverId)
            .Join(
                _db.Patients.IgnoreQueryFilters().Where(p => p.Status == PatientStatus.Active),
                pc => pc.PatientId,
                p => p.Id,
                (pc, p) => new { Link = pc, Patient = p })
            .OrderByDescending(x => x.Link.StartDate)
            .ToListAsync(ct);

        return rows
            .Select(r => new PatientCaregiverWithPatient(r.Link, r.Patient))
            .ToList();
    }

    public async Task<PatientCaregiver?> GetByPatientAndCaregiverAsync(
        Guid patientId,
        Guid caregiverId,
        CancellationToken ct = default)
        => await _db.PatientCaregivers
            .FirstOrDefaultAsync(pc => pc.PatientId == patientId && pc.CaregiverId == caregiverId, ct);

    public async Task AddAsync(PatientCaregiver patientCaregiver, CancellationToken ct = default)
        => await _db.PatientCaregivers.AddAsync(patientCaregiver, ct);

    public void Update(PatientCaregiver patientCaregiver)
        => _db.PatientCaregivers.Update(patientCaregiver);
}

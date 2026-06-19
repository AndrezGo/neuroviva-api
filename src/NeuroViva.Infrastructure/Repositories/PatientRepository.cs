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

    public async Task AddAsync(Patient patient, CancellationToken ct = default)
        => await _db.Patients.AddAsync(patient, ct);

    public void Update(Patient patient) => _db.Patients.Update(patient);
}

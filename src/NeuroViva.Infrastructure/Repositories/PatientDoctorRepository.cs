using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class PatientDoctorRepository : IPatientDoctorRepository
{
    private readonly NeuroVivaDbContext _db;

    public PatientDoctorRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<PatientDoctor?> GetActiveByPatientAsync(Guid patientId, CancellationToken ct = default)
        => await _db.PatientDoctors
            .FirstOrDefaultAsync(pd => pd.PatientId == patientId && pd.IsActive, ct);

    public async Task<PatientDoctor?> GetByPatientAndDoctorAsync(Guid patientId, Guid doctorId, CancellationToken ct = default)
        => await _db.PatientDoctors
            .FirstOrDefaultAsync(pd => pd.PatientId == patientId && pd.DoctorId == doctorId, ct);

    public async Task AddAsync(PatientDoctor link, CancellationToken ct = default)
        => await _db.PatientDoctors.AddAsync(link, ct);

    public void Update(PatientDoctor link)
        => _db.PatientDoctors.Update(link);
}

using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Users;
using NeuroViva.Domain.Users.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class DoctorRepository : IDoctorRepository
{
    private readonly NeuroVivaDbContext _db;

    public DoctorRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Doctor?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId, ct);

    public async Task<Doctor?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Doctors.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<Doctor?> GetByMedicalLicenseAsync(string medicalLicense, CancellationToken ct = default)
        => await _db.Doctors.FirstOrDefaultAsync(d => d.MedicalLicense == medicalLicense, ct);

    public async Task AddAsync(Doctor doctor, CancellationToken ct = default)
        => await _db.Doctors.AddAsync(doctor, ct);
}

using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class PatientDiseaseRepository : IPatientDiseaseRepository
{
    private readonly NeuroVivaDbContext _db;

    public PatientDiseaseRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<IReadOnlyList<PatientDisease>> ListByPatientAsync(
        Guid patientId, CancellationToken ct = default)
    {
        return await _db.PatientDiseases
            .AsNoTracking()
            .Where(pd => pd.PatientId == patientId)
            .ToListAsync(ct);
    }

    public async Task ReplaceForPatientAsync(
        Guid patientId, IReadOnlyCollection<Guid> diseaseIds, CancellationToken ct = default)
    {
        var existing = await _db.PatientDiseases
            .Where(pd => pd.PatientId == patientId)
            .ToListAsync(ct);

        _db.PatientDiseases.RemoveRange(existing);

        foreach (var diseaseId in diseaseIds.Distinct())
            await _db.PatientDiseases.AddAsync(PatientDisease.Assign(patientId, diseaseId), ct);
    }
}

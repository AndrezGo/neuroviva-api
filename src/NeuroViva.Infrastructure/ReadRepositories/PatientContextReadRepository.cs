using Microsoft.EntityFrameworkCore;
using NeuroViva.Application.Ai;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.ReadRepositories;

public sealed class PatientContextReadRepository : IPatientContextReadRepository
{
    private readonly NeuroVivaDbContext _db;

    public PatientContextReadRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<PatientProfileDto?> GetPatientProfileAsync(Guid patientId, CancellationToken ct = default)
    {
        // IgnoreQueryFilters: doctors are cross-tenant and must be able to read any patient.
        var patient = await _db.Patients
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(p => p.Id == patientId)
            .Select(p => new { p.Name, p.DateOfBirth, p.TenantId })
            .FirstOrDefaultAsync(ct);

        if (patient is null)
            return null;

        // Load condition names using the same PatientDiseases -> Diseases join pattern
        // used in DoctorReadRepository.
        var conditionNames = await _db.PatientDiseases
            .AsNoTracking()
            .Where(pd => pd.PatientId == patientId)
            .Join(_db.Diseases, pd => pd.DiseaseId, d => d.Id, (pd, d) => d.Name)
            .ToListAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = patient.DateOfBirth.HasValue
            ? CalculateAge(patient.DateOfBirth.Value, today)
            : 0;

        return new PatientProfileDto(
            Name: patient.Name,
            Age: age,
            Conditions: conditionNames.ToArray(),
            TenantId: patient.TenantId);
    }

    private static int CalculateAge(DateOnly dateOfBirth, DateOnly today)
    {
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
}

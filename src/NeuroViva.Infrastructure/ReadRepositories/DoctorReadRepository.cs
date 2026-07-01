using Microsoft.EntityFrameworkCore;
using NeuroViva.Application.Caregivers.Queries.GetPatientDoctor;
using NeuroViva.Application.Doctors;
using NeuroViva.Application.Doctors.Queries.GetDoctorAlerts;
using NeuroViva.Application.Doctors.Queries.GetDoctorPatients;
using NeuroViva.Application.Doctors.Queries.GetDoctors;
using NeuroViva.Domain.Ai.Enums;
using NeuroViva.Domain.Patients.Enums;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.ReadRepositories;

public sealed class DoctorReadRepository : IDoctorReadRepository
{
    private readonly NeuroVivaDbContext _db;

    public DoctorReadRepository(NeuroVivaDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DoctorPatientDto>> ListPatientsAsync(
        Guid doctorId,
        CancellationToken ct = default)
    {
        // No tenant guard — DoctorId is the security boundary for doctors (they are cross-tenant).
        // IgnoreQueryFilters bypasses any global TenantId filter on Patients.

        // Step 1: Load patients linked to this doctor (active links + active patients only)
        var patientRows = await _db.PatientDoctors
            .AsNoTracking()
            .Where(pd => pd.DoctorId == doctorId && pd.IsActive)
            .Join(
                _db.Patients.IgnoreQueryFilters().Where(p => p.Status == PatientStatus.Active),
                pd => pd.PatientId,
                p => p.Id,
                (pd, p) => new
                {
                    p.Id,
                    p.Name,
                    p.DateOfBirth,
                })
            .ToListAsync(ct);

        if (patientRows.Count == 0)
            return Array.Empty<DoctorPatientDto>();

        // Step 2: Load unresolved alert priorities for those patients under this doctor
        var patientIds = patientRows.Select(r => r.Id).ToList();

        // Step 1b: Load condition names for those patients, grouped by patient
        var conditionRows = await _db.PatientDiseases
            .AsNoTracking()
            .Where(pd => patientIds.Contains(pd.PatientId))
            .Join(
                _db.Diseases,
                pd => pd.DiseaseId,
                d => d.Id,
                (pd, d) => new { pd.PatientId, DiseaseName = d.Name })
            .ToListAsync(ct);

        var conditionsByPatient = conditionRows
            .GroupBy(r => r.PatientId)
            .ToDictionary(g => g.Key, g => g.Select(r => r.DiseaseName).ToList());

        var alertPriorities = await _db.Alerts
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId && !a.Resolved && patientIds.Contains(a.PatientId))
            .Select(a => new { a.PatientId, a.Priority })
            .ToListAsync(ct);

        // Step 2b: Get last activity date per patient across symptoms, appointments, and medication logs
        var lastSymptoms = await _db.Symptoms
            .AsNoTracking()
            .Where(s => patientIds.Contains(s.PatientId))
            .GroupBy(s => s.PatientId)
            .Select(g => new { PatientId = g.Key, At = g.Max(s => s.CreatedAt) })
            .ToListAsync(ct);

        var lastAppointments = await _db.Appointments
            .AsNoTracking()
            .Where(a => patientIds.Contains(a.PatientId))
            .GroupBy(a => a.PatientId)
            .Select(g => new { PatientId = g.Key, At = g.Max(a => a.CreatedAt) })
            .ToListAsync(ct);

        var lastMedLogs = await _db.MedicationLogs
            .AsNoTracking()
            .Join(
                _db.Medications.Where(m => patientIds.Contains(m.PatientId)),
                ml => ml.MedicationId,
                m => m.Id,
                (ml, m) => new { m.PatientId, ml.CreatedAt })
            .GroupBy(x => x.PatientId)
            .Select(g => new { PatientId = g.Key, At = g.Max(x => x.CreatedAt) })
            .ToListAsync(ct);

        var lastActivityByPatient = lastSymptoms
            .Concat(lastAppointments)
            .Concat(lastMedLogs)
            .GroupBy(x => x.PatientId)
            .ToDictionary(g => g.Key, g => (DateTime?)g.Max(x => x.At));

        // Step 3: Group by patient and find highest priority in memory
        var priorityByPatient = alertPriorities
            .GroupBy(a => a.PatientId)
            .ToDictionary(
                g => g.Key,
                g => g.MaxBy(a => PriorityRank(a.Priority))?.Priority);

        // Step 4: Map to DTOs
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return patientRows.Select(row =>
        {
            var age = row.DateOfBirth.HasValue
                ? CalculateAge(row.DateOfBirth.Value, today)
                : 0;

            string? highestAlertPriority = null;
            if (priorityByPatient.TryGetValue(row.Id, out var priority) && priority.HasValue)
                highestAlertPriority = PriorityToString(priority.Value);

            var lastActivity = lastActivityByPatient.TryGetValue(row.Id, out var la) ? la : null;

            var conditions = conditionsByPatient.TryGetValue(row.Id, out var c)
                ? c
                : new List<string>();

            return new DoctorPatientDto(
                PatientId: row.Id,
                Name: row.Name,
                Conditions: conditions,
                ConditionStage: null,
                Age: age,
                HighestAlertPriority: highestAlertPriority,
                LastActivityAt: lastActivity);
        }).ToList();
    }

    public async Task<IReadOnlyList<DoctorAlertDto>> ListAlertsAsync(
        Guid doctorId,
        bool includeResolved = false,
        CancellationToken ct = default)
    {
        // No tenant guard — DoctorId scopes the alerts already.
        // IgnoreQueryFilters bypasses any global TenantId filter on Patients.

        var query = _db.Alerts
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId);

        if (!includeResolved)
            query = query.Where(a => !a.Resolved);

        var rows = await query
            .Join(
                _db.Patients.IgnoreQueryFilters(),
                a => a.PatientId,
                p => p.Id,
                (a, p) => new
                {
                    a.Id,
                    a.PatientId,
                    PatientName = p.Name,
                    a.Type,
                    a.Priority,
                    a.Description,
                    a.Seen,
                    a.Resolved,
                    a.CreatedAt
                })
            .ToListAsync(ct);

        return rows
            .OrderByDescending(x => PriorityRank(x.Priority))
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new DoctorAlertDto(
                Id: x.Id,
                PatientId: x.PatientId,
                PatientName: x.PatientName,
                Type: x.Type,
                Priority: PriorityToString(x.Priority),
                Description: x.Description,
                Seen: x.Seen,
                Resolved: x.Resolved,
                CreatedAt: x.CreatedAt))
            .ToList();
    }

    public async Task<IReadOnlyList<DoctorListItemDto>> ListAllAsync(CancellationToken ct = default)
    {
        // Two separate queries + in-memory merge avoids EF Core cross-DbSet JOIN translation issues.
        // IgnoreQueryFilters on Users because doctor User records may belong to a different tenant.
        var doctors = await _db.Doctors
            .AsNoTracking()
            .Select(d => new { d.Id, d.UserId, d.Specialty, d.MedicalLicense })
            .ToListAsync(ct);

        if (doctors.Count == 0) return Array.Empty<DoctorListItemDto>();

        var userIds = doctors.Select(d => d.UserId).ToList();
        var users = await _db.Users
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Name })
            .ToListAsync(ct);

        var userById = users.ToDictionary(u => u.Id, u => u.Name);

        return doctors
            .Select(d => new DoctorListItemDto(
                DoctorId: d.Id,
                Name: userById.TryGetValue(d.UserId, out var name) ? name : "—",
                Specialty: d.Specialty,
                MedicalLicense: d.MedicalLicense))
            .OrderBy(dto => dto.Name)
            .ToList();
    }

    public async Task<PatientDoctorDto?> GetCurrentDoctorForPatientAsync(
        Guid patientId,
        CancellationToken ct = default)
    {
        var link = await _db.PatientDoctors
            .AsNoTracking()
            .Where(pd => pd.PatientId == patientId && pd.IsActive)
            .Select(pd => new { pd.DoctorId })
            .FirstOrDefaultAsync(ct);

        if (link is null) return null;

        var doctor = await _db.Doctors
            .AsNoTracking()
            .Where(d => d.Id == link.DoctorId)
            .Select(d => new { d.Id, d.UserId, d.Specialty, d.MedicalLicense })
            .FirstOrDefaultAsync(ct);

        if (doctor is null) return null;

        var user = await _db.Users
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(u => u.Id == doctor.UserId)
            .Select(u => new { u.Name })
            .FirstOrDefaultAsync(ct);

        return user is null
            ? null
            : new PatientDoctorDto(doctor.Id, user.Name, doctor.Specialty, doctor.MedicalLicense);
    }

    private static int PriorityRank(AlertPriority priority) => priority switch
    {
        AlertPriority.Critical => 3,
        AlertPriority.High => 2,
        AlertPriority.Medium => 1,
        AlertPriority.Info => 0,
        _ => 0
    };

    private static string PriorityToString(AlertPriority priority) => priority switch
    {
        AlertPriority.Critical => "critica",
        AlertPriority.High => "alta",
        AlertPriority.Medium => "media",
        AlertPriority.Info => "info",
        _ => "info"
    };

    private static int CalculateAge(DateOnly dateOfBirth, DateOnly today)
    {
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
}

using Microsoft.EntityFrameworkCore;
using NeuroViva.Application.Caregivers.Queries.GetPatientDoctor;
using NeuroViva.Application.Common.Abstractions;
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
    private readonly ITenantContext _tenantContext;

    public DoctorReadRepository(NeuroVivaDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<DoctorPatientDto>> ListPatientsAsync(
        Guid doctorId,
        CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId is null) return Array.Empty<DoctorPatientDto>();

        // Step 1: Load patients linked to this doctor (active links + active patients only)
        var patientRows = await _db.PatientDoctors
            .AsNoTracking()
            .Where(pd => pd.DoctorId == doctorId && pd.IsActive)
            .Join(
                _db.Patients.Where(p => p.TenantId == tenantId.Value && p.Status == PatientStatus.Active),
                pd => pd.PatientId,
                p => p.Id,
                (pd, p) => new
                {
                    p.Id,
                    p.Name,
                    p.DateOfBirth,
                    DiseaseName = p.DiseaseId == null
                        ? null
                        : _db.Diseases
                            .Where(d => d.Id == p.DiseaseId)
                            .Select(d => d.Name)
                            .FirstOrDefault()
                })
            .ToListAsync(ct);

        if (patientRows.Count == 0)
            return Array.Empty<DoctorPatientDto>();

        // Step 2: Load unresolved alert priorities for those patients under this doctor
        var patientIds = patientRows.Select(r => r.Id).ToList();

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

            return new DoctorPatientDto(
                PatientId: row.Id,
                Name: row.Name,
                Condition: row.DiseaseName,
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
        var tenantId = _tenantContext.TenantId;
        if (tenantId is null) return Array.Empty<DoctorAlertDto>();

        var query = _db.Alerts
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId);

        if (!includeResolved)
            query = query.Where(a => !a.Resolved);

        var rows = await query
            .Join(
                _db.Patients.Where(p => p.TenantId == tenantId.Value),
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
        return await _db.Doctors
            .AsNoTracking()
            .Join(
                _db.Users,
                d => d.UserId,
                u => u.Id,
                (d, u) => new DoctorListItemDto(d.Id, u.Name, d.Specialty, d.MedicalLicense))
            .OrderBy(dto => dto.Name)
            .ToListAsync(ct);
    }

    public async Task<PatientDoctorDto?> GetCurrentDoctorForPatientAsync(
        Guid patientId,
        CancellationToken ct = default)
    {
        return await _db.PatientDoctors
            .AsNoTracking()
            .Where(pd => pd.PatientId == patientId && pd.IsActive)
            .Join(
                _db.Doctors,
                pd => pd.DoctorId,
                d => d.Id,
                (pd, d) => new { d.Id, d.UserId, d.Specialty, d.MedicalLicense })
            .Join(
                _db.Users,
                x => x.UserId,
                u => u.Id,
                (x, u) => new PatientDoctorDto(x.Id, u.Name, x.Specialty, x.MedicalLicense))
            .FirstOrDefaultAsync(ct);
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

using Microsoft.EntityFrameworkCore;
using NeuroViva.Application.Caregivers;
using NeuroViva.Application.Caregivers.Queries.GetAppointments;
using NeuroViva.Application.Caregivers.Queries.GetMedicationLogs;
using NeuroViva.Application.Caregivers.Queries.GetMedications;
using NeuroViva.Application.Caregivers.Queries.GetPatient;
using NeuroViva.Application.Caregivers.Queries.GetToday;
using NeuroViva.Domain.Appointments.Enums;
using NeuroViva.Domain.Patients.Enums;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.ReadRepositories;

public sealed class CaregiverReadRepository : ICaregiverReadRepository
{
    private readonly NeuroVivaDbContext _db;

    public CaregiverReadRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<CaregiverPatientDto?> GetActivePatientAsync(
        Guid caregiverUserId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        // Join: Caregiver → PatientCaregiver → Patient → Disease
        // Filter patient.tenant_id explicitly (no global filter on these entities through joins)
        var row = await _db.Caregivers
            .AsNoTracking()
            .Where(c => c.UserId == caregiverUserId)
            .Join(
                _db.PatientCaregivers,
                c => c.Id,
                pc => pc.CaregiverId,
                (c, pc) => pc)
            .Join(
                _db.Patients.IgnoreQueryFilters().Where(p =>
                    p.TenantId == tenantId &&
                    p.Status == PatientStatus.Active),
                pc => pc.PatientId,
                p => p.Id,
                (pc, p) => new { Link = pc, Patient = p })
            .OrderByDescending(x => x.Link.StartDate)
            .Select(x => new
            {
                x.Patient.Id,
                x.Patient.Name,
                x.Patient.DateOfBirth,
                DiseaseName = x.Patient.DiseaseId == null
                    ? null
                    : _db.Diseases
                        .Where(d => d.Id == x.Patient.DiseaseId)
                        .Select(d => d.Name)
                        .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (row is null) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = row.DateOfBirth.HasValue
            ? CalculateAge(row.DateOfBirth.Value, today)
            : 0;

        return new CaregiverPatientDto(
            Id: row.Id,
            Name: row.Name,
            Age: age,
            Condition: row.DiseaseName ?? string.Empty,
            // ConditionStage is always null in v1 — schema has no stage column.
            ConditionStage: null);
    }

    public async Task<CaregiverTodayDto?> GetTodayAsync(
        Guid caregiverUserId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        // Resolve the active patient for this caregiver
        var patientId = await _db.Caregivers
            .AsNoTracking()
            .Where(c => c.UserId == caregiverUserId)
            .Join(
                _db.PatientCaregivers,
                c => c.Id,
                pc => pc.CaregiverId,
                (c, pc) => pc)
            .Join(
                _db.Patients.IgnoreQueryFilters().Where(p =>
                    p.TenantId == tenantId &&
                    p.Status == PatientStatus.Active),
                pc => pc.PatientId,
                p => p.Id,
                (pc, p) => new { pc.StartDate, p.Id })
            .OrderByDescending(x => x.StartDate)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(ct);

        // No linked active patient → return null; handler converts to empty arrays
        if (patientId is null) return null;

        var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1); // used only for the "taken today" medication log check

        // Medications: active, started on/before today, not ended before today
        var medications = await _db.Medications
            .AsNoTracking()
            .Where(m =>
                m.PatientId == patientId.Value &&
                m.IsActive &&
                m.StartDate <= todayDate &&
                (m.EndDate == null || m.EndDate >= todayDate))
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Dose,
                // Free-text frequency — exposed as scheduledTime per contract
                m.Frequency,
                // Check for a "taken" log today
                TakenToday = _db.MedicationLogs
                    .Any(l =>
                        l.MedicationId == m.Id &&
                        l.Taken &&
                        l.LoggedAt >= todayStart &&
                        l.LoggedAt < todayEnd)
            })
            .ToListAsync(ct);

        var medicationDtos = medications.Select(m => new TodayMedicationDto(
            Id: m.Id,
            Name: m.Name,
            Dose: m.Dose,
            ScheduledTime: m.Frequency,
            Status: m.TakenToday ? "taken" : "pending",
            // isNow is always false in v1 — no structured schedule exists to determine "is now"
            IsNow: false
        )).ToList();

        // Appointments: upcoming from today onwards, status scheduled or confirmed, ordered by scheduled_at, top 5
        // Title computation (string indexing) is done client-side after projection.
        var rawAppointments = await _db.Appointments
            .AsNoTracking()
            .Where(a =>
                a.PatientId == patientId.Value &&
                a.ScheduledAt >= todayStart &&
                (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed))
            .OrderBy(a => a.ScheduledAt)
            .Take(5)
            .Select(a => new
            {
                a.Id,
                a.Notes,
                a.Type,
                a.ScheduledAt
            })
            .ToListAsync(ct);

        var appointments = rawAppointments.Select(a =>
        {
            var typeStr = a.Type.ToString();
            var title = !string.IsNullOrEmpty(a.Notes)
                ? a.Notes
                : char.ToUpperInvariant(typeStr[0]) + typeStr[1..].ToLowerInvariant();

            return new TodayAppointmentDto(
                Id: a.Id,
                Title: title,
                Type: typeStr.ToLowerInvariant(),
                ScheduledAt: a.ScheduledAt.ToString("o")
            );
        }).ToList();

        return new CaregiverTodayDto(
            Medications: medicationDtos,
            Appointments: appointments);
    }

    public async Task<IReadOnlyList<MedicationListItemDto>> ListMedicationsAsync(
        Guid caregiverUserId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var patientId = await ResolveActivePatientIdAsync(caregiverUserId, tenantId, ct);
        if (patientId is null) return Array.Empty<MedicationListItemDto>();

        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var rows = await _db.Medications
            .AsNoTracking()
            .Where(m => m.PatientId == patientId.Value)
            .OrderByDescending(m => m.IsActive)
            .ThenByDescending(m => m.CreatedAt)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Dose,
                m.Frequency,
                m.IsActive,
                m.StartDate,
                m.EndDate,
                m.CreatedAt,
                TakenToday = _db.MedicationLogs
                    .Any(l =>
                        l.MedicationId == m.Id &&
                        l.Taken &&
                        l.LoggedAt >= todayStart &&
                        l.LoggedAt < todayEnd)
            })
            .ToListAsync(ct);

        return rows.Select(m => new MedicationListItemDto(
            Id: m.Id,
            Name: m.Name,
            Dose: m.Dose,
            Frequency: m.Frequency,
            Active: m.IsActive,
            StartDate: m.StartDate.ToString("yyyy-MM-dd"),
            EndDate: m.EndDate?.ToString("yyyy-MM-dd"),
            CreatedAt: m.CreatedAt.ToString("o"),
            TakenToday: m.TakenToday)).ToList();
    }

    public async Task<IReadOnlyList<AppointmentListItemDto>> ListAppointmentsAsync(
        Guid caregiverUserId,
        Guid tenantId,
        CancellationToken ct = default,
        int take = 50)
    {
        var patientId = await ResolveActivePatientIdAsync(caregiverUserId, tenantId, ct);
        if (patientId is null) return Array.Empty<AppointmentListItemDto>();

        var rows = await _db.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId.Value)
            .Select(a => new
            {
                a.Id,
                a.Notes,
                a.Type,
                a.ScheduledAt,
                a.Status
            })
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        // Future appointments ascending first, then past appointments descending.
        var ordered = rows
            .Where(a => a.ScheduledAt >= now)
            .OrderBy(a => a.ScheduledAt)
            .Concat(rows
                .Where(a => a.ScheduledAt < now)
                .OrderByDescending(a => a.ScheduledAt))
            .Take(take);

        return ordered.Select(a =>
        {
            var typeStr = a.Type.ToString();
            var title = !string.IsNullOrWhiteSpace(a.Notes)
                ? a.Notes.Split('\n')[0].Trim()
                : char.ToUpperInvariant(typeStr[0]) + typeStr[1..].ToLowerInvariant();

            return new AppointmentListItemDto(
                Id: a.Id,
                Title: title,
                Type: typeStr.ToLowerInvariant(),
                ScheduledAt: a.ScheduledAt.ToString("o"),
                Status: a.Status.ToString().ToLowerInvariant());
        }).ToList();
    }

    public async Task<IReadOnlyList<MedicationLogItemDto>> ListMedicationLogsAsync(
        Guid caregiverUserId,
        Guid tenantId,
        Guid medicationId,
        CancellationToken ct = default,
        int take = 200)
    {
        var rows = await _db.MedicationLogs
            .AsNoTracking()
            .Where(l => l.MedicationId == medicationId)
            .OrderByDescending(l => l.LoggedAt)
            .Take(take)
            .Select(l => new
            {
                l.Id,
                l.Taken,
                l.LoggedAt,
                l.Notes,
                l.LoggedBy
            })
            .ToListAsync(ct);

        return rows.Select(l => new MedicationLogItemDto(
            Id: l.Id,
            Taken: l.Taken,
            LoggedAt: l.LoggedAt.ToString("o"),
            Notes: l.Notes,
            LoggedBy: l.LoggedBy)).ToList();
    }

    /// <summary>
    /// Resolves the active patient id linked to the caregiver, or null if none.
    /// </summary>
    private async Task<Guid?> ResolveActivePatientIdAsync(
        Guid caregiverUserId,
        Guid tenantId,
        CancellationToken ct)
    {
        return await _db.Caregivers
            .AsNoTracking()
            .Where(c => c.UserId == caregiverUserId)
            .Join(
                _db.PatientCaregivers,
                c => c.Id,
                pc => pc.CaregiverId,
                (c, pc) => pc)
            .Join(
                _db.Patients.IgnoreQueryFilters().Where(p =>
                    p.TenantId == tenantId &&
                    p.Status == PatientStatus.Active),
                pc => pc.PatientId,
                p => p.Id,
                (pc, p) => new { pc.StartDate, p.Id })
            .OrderByDescending(x => x.StartDate)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(ct);
    }

    private static int CalculateAge(DateOnly dateOfBirth, DateOnly today)
    {
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
}

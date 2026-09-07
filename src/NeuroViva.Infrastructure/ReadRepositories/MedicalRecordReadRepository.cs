using Microsoft.EntityFrameworkCore;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Options;
using NeuroViva.Application.MedicalRecords;
using NeuroViva.Application.MedicalRecords.Queries;
using NeuroViva.Application.MedicalRecords.Queries.GetFollowUp;
using NeuroViva.Domain.Appointments.Enums;
using NeuroViva.Domain.Patients.Enums;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.ReadRepositories;

public sealed class MedicalRecordReadRepository : IMedicalRecordReadRepository
{
    private readonly NeuroVivaDbContext _db;
    private readonly IStorageService _storageService;
    private readonly StorageOptions _storageOptions;

    public MedicalRecordReadRepository(
        NeuroVivaDbContext db,
        IStorageService storageService,
        StorageOptions storageOptions)
    {
        _db = db;
        _storageService = storageService;
        _storageOptions = storageOptions;
    }

    public async Task<IReadOnlyList<ClinicalRecordDto>> ListExamsAsync(
        Guid patientId,
        CancellationToken ct = default)
    {
        var records = await _db.ClinicalRecords
            .AsNoTracking()
            .Include(r => r.Attachments)
            .Where(c => c.PatientId == patientId && c.EventType == ClinicalEventType.Exam)
            .OrderByDescending(c => c.EventDate)
            .Take(100)
            .ToListAsync(ct);

        var dtos = new List<ClinicalRecordDto>(records.Count);
        foreach (var record in records)
        {
            var attachmentDtos = new List<ClinicalRecordAttachmentDto>(record.Attachments.Count);
            foreach (var att in record.Attachments)
            {
                var signedUrl = await _storageService.GetSignedUrlAsync(
                    _storageOptions.AttachmentsBucket,
                    att.StoragePath,
                    TimeSpan.FromSeconds(_storageOptions.SignedUrlExpirySeconds),
                    ct);

                attachmentDtos.Add(new ClinicalRecordAttachmentDto(
                    Id: att.Id,
                    FileName: att.FileName,
                    ContentType: att.ContentType,
                    SignedUrl: signedUrl));
            }

            dtos.Add(new ClinicalRecordDto(
                Id: record.Id,
                EventType: "exam",
                Description: record.Description,
                EventDate: record.EventDate.ToString("o"),
                Attachments: attachmentDtos,
                CreatedAt: record.CreatedAt));
        }

        return dtos;
    }

    public async Task<IReadOnlyList<ClinicalRecordDto>> ListClinicalNotesAsync(
        Guid patientId,
        CancellationToken ct = default)
    {
        var records = await _db.ClinicalRecords
            .AsNoTracking()
            .Include(r => r.Attachments)
            .Where(c => c.PatientId == patientId &&
                        (c.EventType == ClinicalEventType.Consultation ||
                         c.EventType == ClinicalEventType.Note ||
                         c.EventType == ClinicalEventType.Other))
            .OrderByDescending(c => c.EventDate)
            .Take(100)
            .ToListAsync(ct);

        var dtos = new List<ClinicalRecordDto>(records.Count);
        foreach (var record in records)
        {
            var attachmentDtos = new List<ClinicalRecordAttachmentDto>(record.Attachments.Count);
            foreach (var att in record.Attachments)
            {
                var signedUrl = await _storageService.GetSignedUrlAsync(
                    _storageOptions.AttachmentsBucket,
                    att.StoragePath,
                    TimeSpan.FromSeconds(_storageOptions.SignedUrlExpirySeconds),
                    ct);

                attachmentDtos.Add(new ClinicalRecordAttachmentDto(
                    Id: att.Id,
                    FileName: att.FileName,
                    ContentType: att.ContentType,
                    SignedUrl: signedUrl));
            }

            var eventTypeStr = record.EventType switch
            {
                ClinicalEventType.Consultation => "consultation",
                ClinicalEventType.Note => "note",
                _ => "other"
            };

            dtos.Add(new ClinicalRecordDto(
                Id: record.Id,
                EventType: eventTypeStr,
                Description: record.Description,
                EventDate: record.EventDate.ToString("o"),
                Attachments: attachmentDtos,
                CreatedAt: record.CreatedAt));
        }

        return dtos;
    }

    public async Task<IReadOnlyList<HistoryEventDto>> ListFollowUpAsync(
        Guid patientId,
        CancellationToken ct = default)
    {
        var rawEvents = new List<(DateTime When, HistoryEventDto Dto)>();

        // Symptoms
        var symptomRows = await _db.Symptoms
            .AsNoTracking()
            .Where(s => s.PatientId == patientId && !s.IsDeleted)
            .OrderByDescending(s => s.LoggedAt)
            .Take(100)
            .Select(s => new { s.Id, s.Type, s.Description, s.LoggedAt })
            .ToListAsync(ct);

        foreach (var s in symptomRows)
        {
            rawEvents.Add((s.LoggedAt, new HistoryEventDto(
                Id: s.Id,
                Type: "symptom",
                Title: !string.IsNullOrWhiteSpace(s.Type) ? s.Type : "Síntoma",
                Description: s.Description,
                EventDate: s.LoggedAt.ToString("o"),
                Status: null,
                AttachmentUrl: null,
                AttachmentFileName: null)));
        }

        // Appointments
        var appointmentRows = await _db.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.ScheduledAt)
            .Take(100)
            .Select(a => new { a.Id, a.Type, a.Notes, a.ScheduledAt, a.Status })
            .ToListAsync(ct);

        foreach (var a in appointmentRows)
        {
            var typeStr = a.Type.ToString();
            var title = !string.IsNullOrWhiteSpace(a.Notes)
                ? a.Notes.Split('\n')[0].Trim()
                : char.ToUpperInvariant(typeStr[0]) + typeStr[1..].ToLowerInvariant();

            string? description = null;
            if (!string.IsNullOrWhiteSpace(a.Notes))
            {
                var parts = a.Notes.Split('\n', 2);
                if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                    description = parts[1].Trim();
            }

            rawEvents.Add((a.ScheduledAt, new HistoryEventDto(
                Id: a.Id,
                Type: typeStr.ToLowerInvariant(),
                Title: title,
                Description: description,
                EventDate: a.ScheduledAt.ToString("o"),
                Status: a.Status.ToString().ToLowerInvariant(),
                AttachmentUrl: null,
                AttachmentFileName: null)));
        }

        // Medication logs — only "taken" entries are meaningful in history
        var medLogRows = await _db.MedicationLogs
            .AsNoTracking()
            .Where(l => l.Taken)
            .Join(
                _db.Medications.Where(m => m.PatientId == patientId),
                l => l.MedicationId,
                m => m.Id,
                (l, m) => new { l.Id, MedicationName = m.Name, l.Notes, l.LoggedAt })
            .OrderByDescending(x => x.LoggedAt)
            .Take(100)
            .ToListAsync(ct);

        foreach (var l in medLogRows)
        {
            rawEvents.Add((l.LoggedAt, new HistoryEventDto(
                Id: l.Id,
                Type: "medication",
                Title: l.MedicationName,
                Description: l.Notes,
                EventDate: l.LoggedAt.ToString("o"),
                Status: null,
                AttachmentUrl: null,
                AttachmentFileName: null)));
        }

        return rawEvents
            .OrderByDescending(x => x.When)
            .Take(100)
            .Select(x => x.Dto)
            .ToList();
    }

    // -------------------------------------------------------------------------
    // Plain-text variants for AI context building (no signed URLs, no attachments)
    // -------------------------------------------------------------------------

    public async Task<IReadOnlyList<ClinicalRecordTextDto>> ListExamsTextAsync(
        Guid patientId,
        int limit,
        CancellationToken ct = default)
    {
        var records = await _db.ClinicalRecords
            .AsNoTracking()
            .Include(c => c.Attachments)
            .Where(c => c.PatientId == patientId && c.EventType == ClinicalEventType.Exam)
            .OrderByDescending(c => c.EventDate)
            .Take(limit)
            .ToListAsync(ct);

        return records.Select(c => new ClinicalRecordTextDto(
            Id: c.Id,
            EventType: "exam",
            Description: c.Description,
            EventDate: c.EventDate,
            Attachments: c.Attachments
                .Select(a => new ClinicalRecordAttachmentTextDto(a.FileName, a.ContentType, a.ExtractedText))
                .ToList())).ToList();
    }

    public async Task<IReadOnlyList<ClinicalRecordTextDto>> ListClinicalNotesTextAsync(
        Guid patientId,
        int limit,
        CancellationToken ct = default)
    {
        var records = await _db.ClinicalRecords
            .AsNoTracking()
            .Include(c => c.Attachments)
            .Where(c => c.PatientId == patientId &&
                        (c.EventType == ClinicalEventType.Consultation ||
                         c.EventType == ClinicalEventType.Note ||
                         c.EventType == ClinicalEventType.Other))
            .OrderByDescending(c => c.EventDate)
            .Take(limit)
            .ToListAsync(ct);

        return records.Select(c => new ClinicalRecordTextDto(
            Id: c.Id,
            EventType: c.EventType switch
            {
                ClinicalEventType.Consultation => "consultation",
                ClinicalEventType.Note => "note",
                _ => "other"
            },
            Description: c.Description,
            EventDate: c.EventDate,
            Attachments: c.Attachments
                .Select(a => new ClinicalRecordAttachmentTextDto(a.FileName, a.ContentType, a.ExtractedText))
                .ToList())).ToList();
    }

    public async Task<IReadOnlyList<HistoryEventTextDto>> ListFollowUpTextAsync(
        Guid patientId,
        int limit,
        CancellationToken ct = default)
    {
        var rawEvents = new List<(DateTime When, HistoryEventTextDto Dto)>();

        // Symptoms
        var symptomRows = await _db.Symptoms
            .AsNoTracking()
            .Where(s => s.PatientId == patientId && !s.IsDeleted)
            .OrderByDescending(s => s.LoggedAt)
            .Take(limit)
            .Select(s => new { s.Id, s.Type, s.Description, s.LoggedAt })
            .ToListAsync(ct);

        foreach (var s in symptomRows)
        {
            rawEvents.Add((s.LoggedAt, new HistoryEventTextDto(
                Id: s.Id,
                Type: "symptom",
                Title: !string.IsNullOrWhiteSpace(s.Type) ? s.Type : "Síntoma",
                Description: s.Description,
                EventDate: s.LoggedAt,
                Status: null)));
        }

        // Appointments
        var appointmentRows = await _db.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.ScheduledAt)
            .Take(limit)
            .Select(a => new { a.Id, a.Type, a.Notes, a.ScheduledAt, a.Status })
            .ToListAsync(ct);

        foreach (var a in appointmentRows)
        {
            var typeStr = a.Type.ToString();
            var title = !string.IsNullOrWhiteSpace(a.Notes)
                ? a.Notes.Split('\n')[0].Trim()
                : char.ToUpperInvariant(typeStr[0]) + typeStr[1..].ToLowerInvariant();

            string? description = null;
            if (!string.IsNullOrWhiteSpace(a.Notes))
            {
                var parts = a.Notes.Split('\n', 2);
                if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                    description = parts[1].Trim();
            }

            rawEvents.Add((a.ScheduledAt, new HistoryEventTextDto(
                Id: a.Id,
                Type: typeStr.ToLowerInvariant(),
                Title: title,
                Description: description,
                EventDate: a.ScheduledAt,
                Status: a.Status.ToString().ToLowerInvariant())));
        }

        // Medication logs — "taken" entries only
        var medLogRows = await _db.MedicationLogs
            .AsNoTracking()
            .Where(l => l.Taken)
            .Join(
                _db.Medications.Where(m => m.PatientId == patientId),
                l => l.MedicationId,
                m => m.Id,
                (l, m) => new { l.Id, MedicationName = m.Name, l.Notes, l.LoggedAt })
            .OrderByDescending(x => x.LoggedAt)
            .Take(limit)
            .ToListAsync(ct);

        foreach (var l in medLogRows)
        {
            rawEvents.Add((l.LoggedAt, new HistoryEventTextDto(
                Id: l.Id,
                Type: "medication",
                Title: l.MedicationName,
                Description: l.Notes,
                EventDate: l.LoggedAt,
                Status: null)));
        }

        return rawEvents
            .OrderByDescending(x => x.When)
            .Take(limit)
            .Select(x => x.Dto)
            .ToList();
    }
}

using NeuroViva.Application.MedicalRecords.Queries;
using NeuroViva.Application.MedicalRecords.Queries.GetFollowUp;

namespace NeuroViva.Application.MedicalRecords;

public interface IMedicalRecordReadRepository
{
    Task<IReadOnlyList<ClinicalRecordDto>> ListExamsAsync(Guid patientId, CancellationToken ct = default);
    Task<IReadOnlyList<ClinicalRecordDto>> ListClinicalNotesAsync(Guid patientId, CancellationToken ct = default);
    Task<IReadOnlyList<HistoryEventDto>> ListFollowUpAsync(Guid patientId, CancellationToken ct = default);

    // Plain-text variants for AI context building — no signed URLs, no attachments.
    Task<IReadOnlyList<ClinicalRecordTextDto>> ListExamsTextAsync(Guid patientId, int limit, CancellationToken ct = default);
    Task<IReadOnlyList<ClinicalRecordTextDto>> ListClinicalNotesTextAsync(Guid patientId, int limit, CancellationToken ct = default);
    Task<IReadOnlyList<HistoryEventTextDto>> ListFollowUpTextAsync(Guid patientId, int limit, CancellationToken ct = default);
}

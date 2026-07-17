using NeuroViva.Application.MedicalRecords.Queries;
using NeuroViva.Application.MedicalRecords.Queries.GetFollowUp;

namespace NeuroViva.Application.MedicalRecords;

public interface IMedicalRecordReadRepository
{
    Task<IReadOnlyList<ClinicalRecordDto>> ListExamsAsync(Guid patientId, CancellationToken ct = default);
    Task<IReadOnlyList<ClinicalRecordDto>> ListClinicalNotesAsync(Guid patientId, CancellationToken ct = default);
    Task<IReadOnlyList<HistoryEventDto>> ListFollowUpAsync(Guid patientId, CancellationToken ct = default);
}

namespace NeuroViva.Application.Ai;

public interface IPatientContextReadRepository
{
    Task<PatientProfileDto?> GetPatientProfileAsync(Guid patientId, CancellationToken ct = default);
}

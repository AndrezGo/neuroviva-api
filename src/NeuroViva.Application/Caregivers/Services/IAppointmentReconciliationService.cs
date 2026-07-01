namespace NeuroViva.Application.Caregivers.Services;

public interface IAppointmentReconciliationService
{
    Task ReconcileForPatientAsync(Guid patientId, CancellationToken ct = default);
}

using NeuroViva.Application.Caregivers.Queries.GetAppointments;
using NeuroViva.Application.Caregivers.Queries.GetMedicationLogs;
using NeuroViva.Application.Caregivers.Queries.GetMedications;
using NeuroViva.Application.Caregivers.Queries.GetPatient;
using NeuroViva.Application.Caregivers.Queries.GetToday;

namespace NeuroViva.Application.Caregivers;

public interface ICaregiverReadRepository
{
    /// <summary>
    /// Returns the caregiver row for the given internal user id, or null if not found.
    /// </summary>
    Task<CaregiverPatientDto?> GetActivePatientAsync(
        Guid caregiverUserId,
        Guid tenantId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns today's medications and appointments for the caregiver's linked active patient.
    /// Returns null when the caregiver has no linked active patient.
    /// </summary>
    Task<CaregiverTodayDto?> GetTodayAsync(
        Guid caregiverUserId,
        Guid tenantId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all medications for the caregiver's linked active patient,
    /// ordered by active status descending then created_at descending.
    /// Returns an empty list when the caregiver has no linked active patient.
    /// </summary>
    Task<IReadOnlyList<MedicationListItemDto>> ListMedicationsAsync(
        Guid caregiverUserId,
        Guid tenantId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns appointments for the caregiver's linked active patient.
    /// Ordering: future appointments ascending by scheduled_at first, then past appointments descending.
    /// Capped at <paramref name="take"/> records to avoid saturating the client.
    /// Returns an empty list when the caregiver has no linked active patient.
    /// </summary>
    Task<IReadOnlyList<AppointmentListItemDto>> ListAppointmentsAsync(
        Guid caregiverUserId,
        Guid tenantId,
        CancellationToken ct = default,
        int take = 50);

    Task<IReadOnlyList<MedicationLogItemDto>> ListMedicationLogsAsync(
        Guid caregiverUserId,
        Guid tenantId,
        Guid medicationId,
        CancellationToken ct = default,
        int take = 200);
}

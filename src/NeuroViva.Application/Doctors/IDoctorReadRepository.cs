using NeuroViva.Application.Caregivers.Queries.GetPatientDoctor;
using NeuroViva.Application.Doctors.Queries.GetDoctorAlerts;
using NeuroViva.Application.Doctors.Queries.GetDoctorPatients;
using NeuroViva.Application.Doctors.Queries.GetDoctors;

namespace NeuroViva.Application.Doctors;

public interface IDoctorReadRepository
{
    Task<IReadOnlyList<DoctorPatientDto>> ListPatientsAsync(Guid doctorId, CancellationToken ct = default);
    Task<IReadOnlyList<DoctorAlertDto>> ListAlertsAsync(Guid doctorId, bool includeResolved = false, CancellationToken ct = default);
    Task<IReadOnlyList<DoctorListItemDto>> ListAllAsync(CancellationToken ct = default);
    Task<PatientDoctorDto?> GetCurrentDoctorForPatientAsync(Guid patientId, CancellationToken ct = default);
}

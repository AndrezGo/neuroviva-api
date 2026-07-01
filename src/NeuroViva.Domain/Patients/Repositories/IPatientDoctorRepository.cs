namespace NeuroViva.Domain.Patients.Repositories;

public interface IPatientDoctorRepository
{
    Task<PatientDoctor?> GetActiveByPatientAsync(Guid patientId, CancellationToken ct = default);
    Task<PatientDoctor?> GetByPatientAndDoctorAsync(Guid patientId, Guid doctorId, CancellationToken ct = default);
    Task AddAsync(PatientDoctor link, CancellationToken ct = default);
    void Update(PatientDoctor link);
}

namespace NeuroViva.Domain.Appointments.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> ListByPatientAsync(Guid patientId, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> ListByDoctorAsync(Guid doctorId, CancellationToken ct = default);
    Task AddAsync(Appointment appointment, CancellationToken ct = default);
    void Update(Appointment appointment);
}

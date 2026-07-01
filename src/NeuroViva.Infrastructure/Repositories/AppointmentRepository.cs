using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Appointments;
using NeuroViva.Domain.Appointments.Enums;
using NeuroViva.Domain.Appointments.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly NeuroVivaDbContext _db;

    public AppointmentRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Appointment>> ListByPatientAsync(Guid patientId, CancellationToken ct = default)
        => await _db.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> ListByDoctorAsync(Guid doctorId, CancellationToken ct = default)
        => await _db.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> ListPendingOutcomeByPatientAsync(
        Guid patientId, DateTime scheduledBefore, CancellationToken ct = default)
        => await _db.Appointments
            .Where(a => a.PatientId == patientId
                     && (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed)
                     && a.ScheduledAt < scheduledBefore)
            .ToListAsync(ct);

    public async Task AddAsync(Appointment appointment, CancellationToken ct = default)
        => await _db.Appointments.AddAsync(appointment, ct);

    public void Update(Appointment appointment) => _db.Appointments.Update(appointment);
}

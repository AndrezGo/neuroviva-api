using Microsoft.Extensions.Logging;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Appointments.Enums;
using NeuroViva.Domain.Appointments.Repositories;

namespace NeuroViva.Application.Caregivers.Services;

public sealed class AppointmentReconciliationService : IAppointmentReconciliationService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<AppointmentReconciliationService> _logger;

    public AppointmentReconciliationService(
        IAppointmentRepository appointmentRepository,
        IUnitOfWork uow,
        ILogger<AppointmentReconciliationService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _uow = uow;
        _logger = logger;
    }

    public async Task ReconcileForPatientAsync(Guid patientId, CancellationToken ct = default)
    {
        // Appointments whose scheduled time passed more than 2 hours ago
        // and whose outcome was never recorded are auto-marked as missed.
        var cutoff = DateTime.UtcNow.AddHours(-2);

        var pending = await _appointmentRepository
            .ListPendingOutcomeByPatientAsync(patientId, cutoff, ct);

        if (pending.Count == 0)
            return;

        var processed = 0;
        foreach (var appointment in pending)
        {
            var domainResult = appointment.MarkAsMissed(AppointmentMissReason.AutoTimeout);
            if (domainResult.IsSuccess)
            {
                _appointmentRepository.Update(appointment);
                processed++;
            }
            else
            {
                _logger.LogWarning(
                    "Could not auto-mark appointment {AppointmentId} as missed: {ErrorCode} — {ErrorMessage}",
                    appointment.Id, domainResult.ErrorCode, domainResult.ErrorMessage);
            }
        }

        if (processed > 0)
            await _uow.SaveChangesAsync(ct);
    }
}

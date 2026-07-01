using MediatR;
using Microsoft.Extensions.Logging;
using NeuroViva.Application.Common.DomainEvents;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Enums;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Domain.Appointments.Events;
using NeuroViva.Domain.Patients.Repositories;

namespace NeuroViva.Application.Ai.EventHandlers;

public sealed class AppointmentCancelledAlertHandler
    : INotificationHandler<DomainEventNotification<AppointmentCancelledDomainEvent>>
{
    private readonly IPatientDoctorRepository _patientDoctorRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AppointmentCancelledAlertHandler> _logger;

    public AppointmentCancelledAlertHandler(
        IPatientDoctorRepository patientDoctorRepository,
        IAlertRepository alertRepository,
        IUnitOfWork unitOfWork,
        ILogger<AppointmentCancelledAlertHandler> logger)
    {
        _patientDoctorRepository = patientDoctorRepository;
        _alertRepository = alertRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<AppointmentCancelledDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        try
        {
            var evt = notification.Event;

            var link = await _patientDoctorRepository.GetActiveByPatientAsync(evt.PatientId, cancellationToken);
            if (link is null)
            {
                _logger.LogDebug(
                    "No active doctor link found for patient {PatientId}. Skipping appointment cancelled alert.",
                    evt.PatientId);
                return;
            }

            // Deduplicate by source reference: one alert per cancelled appointment
            var exists = await _alertRepository.ExistsForSourceAsync(evt.AppointmentId, cancellationToken);
            if (exists)
            {
                _logger.LogDebug(
                    "Alert already exists for appointment {AppointmentId}. Skipping duplicate.",
                    evt.AppointmentId);
                return;
            }

            var fecha = evt.ScheduledAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            var description = $"La cita de tipo '{evt.AppointmentTitle}' programada para el {fecha} fue cancelada.";
            var alert = Alert.Create(
                evt.PatientId, link.DoctorId, "cita", AlertPriority.Info, description,
                sourceReferenceId: evt.AppointmentId);

            await _alertRepository.AddAsync(alert, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Alert created for patient {PatientId} — appointment '{AppointmentTitle}' cancelled.",
                evt.PatientId, evt.AppointmentTitle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create alert for cancelled appointment. AppointmentId={AppointmentId}.",
                notification.Event.AppointmentId);
        }
    }
}

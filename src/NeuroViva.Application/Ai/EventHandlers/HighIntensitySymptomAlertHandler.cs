using MediatR;
using Microsoft.Extensions.Logging;
using NeuroViva.Application.Common.DomainEvents;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Enums;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Domain.HealthMonitoring.Events;
using NeuroViva.Domain.Patients.Repositories;

namespace NeuroViva.Application.Ai.EventHandlers;

public sealed class HighIntensitySymptomAlertHandler
    : INotificationHandler<DomainEventNotification<HighIntensitySymptomDomainEvent>>
{
    private readonly IPatientDoctorRepository _patientDoctorRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HighIntensitySymptomAlertHandler> _logger;

    public HighIntensitySymptomAlertHandler(
        IPatientDoctorRepository patientDoctorRepository,
        IAlertRepository alertRepository,
        IUnitOfWork unitOfWork,
        ILogger<HighIntensitySymptomAlertHandler> logger)
    {
        _patientDoctorRepository = patientDoctorRepository;
        _alertRepository = alertRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<HighIntensitySymptomDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        try
        {
            var evt = notification.Event;

            var link = await _patientDoctorRepository.GetActiveByPatientAsync(evt.PatientId, cancellationToken);
            if (link is null)
            {
                _logger.LogDebug(
                    "No active doctor link found for patient {PatientId}. Skipping high-intensity symptom alert.",
                    evt.PatientId);
                return;
            }

            var priority = evt.Intensity >= 9 ? AlertPriority.Critical : AlertPriority.High;

            var exists = await _alertRepository.ExistsRecentAsync(
                evt.PatientId, "sintoma", priority, TimeSpan.FromMinutes(30), cancellationToken);
            if (exists)
            {
                _logger.LogDebug(
                    "Duplicate symptom alert suppressed for patient {PatientId} within deduplication window.",
                    evt.PatientId);
                return;
            }

            var description = $"Síntoma '{evt.SymptomType}' registrado con intensidad {evt.Intensity}/10.";
            var alert = Alert.Create(
                evt.PatientId, link.DoctorId, "sintoma", priority, description,
                sourceReferenceId: evt.SymptomId);

            await _alertRepository.AddAsync(alert, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Alert created for patient {PatientId} — symptom '{SymptomType}' with intensity {Intensity}.",
                evt.PatientId, evt.SymptomType, evt.Intensity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create alert for high-intensity symptom. PatientId={PatientId}.",
                notification.Event.PatientId);
        }
    }
}

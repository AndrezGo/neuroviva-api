using MediatR;
using Microsoft.Extensions.Logging;
using NeuroViva.Application.Common.DomainEvents;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Ai.Enums;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Domain.Medications.Events;
using NeuroViva.Domain.Patients.Repositories;

namespace NeuroViva.Application.Ai.EventHandlers;

public sealed class MedicationDoseSkippedAlertHandler
    : INotificationHandler<DomainEventNotification<MedicationDoseSkippedDomainEvent>>
{
    private readonly IPatientDoctorRepository _patientDoctorRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MedicationDoseSkippedAlertHandler> _logger;

    public MedicationDoseSkippedAlertHandler(
        IPatientDoctorRepository patientDoctorRepository,
        IAlertRepository alertRepository,
        IUnitOfWork unitOfWork,
        ILogger<MedicationDoseSkippedAlertHandler> logger)
    {
        _patientDoctorRepository = patientDoctorRepository;
        _alertRepository = alertRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<MedicationDoseSkippedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        try
        {
            var evt = notification.Event;

            var link = await _patientDoctorRepository.GetActiveByPatientAsync(evt.PatientId, cancellationToken);
            if (link is null)
            {
                _logger.LogDebug(
                    "No active doctor link found for patient {PatientId}. Skipping medication dose alert.",
                    evt.PatientId);
                return;
            }

            var exists = await _alertRepository.ExistsRecentAsync(
                evt.PatientId, "medicacion", AlertPriority.Medium, TimeSpan.FromMinutes(60), cancellationToken);
            if (exists)
            {
                _logger.LogDebug(
                    "Duplicate medication alert suppressed for patient {PatientId} within deduplication window.",
                    evt.PatientId);
                return;
            }

            var description = $"Dosis de '{evt.MedicationName}' no fue tomada.";
            var alert = Alert.Create(
                evt.PatientId, link.DoctorId, "medicacion", AlertPriority.Medium, description,
                sourceReferenceId: evt.MedicationLogId);

            await _alertRepository.AddAsync(alert, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Alert created for patient {PatientId} — medication dose '{MedicationName}' skipped.",
                evt.PatientId, evt.MedicationName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create alert for skipped medication dose. PatientId={PatientId}.",
                notification.Event.PatientId);
        }
    }
}

using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Medications;
using NeuroViva.Domain.Medications.Repositories;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Commands.LogMedicationDose;

public sealed class LogMedicationDoseCommandHandler
    : IRequestHandler<LogMedicationDoseCommand, Result<LogMedicationDoseResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IMedicationRepository _medicationRepo;
    private readonly IMedicationLogRepository _medicationLogRepo;
    private readonly IUnitOfWork _uow;

    public LogMedicationDoseCommandHandler(
        ICurrentUserService currentUser,
        ICaregiverRepository caregiverRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        IMedicationRepository medicationRepo,
        IMedicationLogRepository medicationLogRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _caregiverRepo = caregiverRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _medicationRepo = medicationRepo;
        _medicationLogRepo = medicationLogRepo;
        _uow = uow;
    }

    public async Task<Result<LogMedicationDoseResult>> Handle(
        LogMedicationDoseCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        // Resolve caregiver profile
        var caregiver = await _caregiverRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (caregiver is null)
            return Error.NotFound("caregiver.not_found", "Caregiver profile not found");

        // Resolve medication — 404 if not found
        var medication = await _medicationRepo.GetByIdAsync(request.MedicationId, cancellationToken);
        if (medication is null)
            return Error.NotFound("medication.not_found", "Medication not found");

        // 403 if the medication does not belong to any patient linked to this caregiver
        var links = await _patientCaregiverRepo.GetActiveByCaregiverAsync(caregiver.Id, cancellationToken);
        var isLinked = links.Any(l => l.Patient.Id == medication.PatientId);

        if (!isLinked)
            return Error.Forbidden("Medication does not belong to a patient linked to this caregiver");

        // Record the dose using the taken flag from the request
        var log = MedicationLog.Record(
            medicationId: request.MedicationId,
            loggedBy: _currentUser.UserId.Value,
            taken: request.Taken ?? true,
            patientId: medication.PatientId,
            medicationName: medication.Name,
            notes: request.Notes,
            loggedAt: DateTime.UtcNow);

        await _medicationLogRepo.AddAsync(log, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new LogMedicationDoseResult(log.Id);
    }
}

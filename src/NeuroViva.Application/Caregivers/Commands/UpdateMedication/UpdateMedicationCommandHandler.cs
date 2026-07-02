using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Medications.Repositories;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Commands.UpdateMedication;

public sealed class UpdateMedicationCommandHandler
    : IRequestHandler<UpdateMedicationCommand, Result>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IMedicationRepository _medicationRepo;
    private readonly IUnitOfWork _uow;

    public UpdateMedicationCommandHandler(
        ICurrentUserService currentUser,
        ICaregiverRepository caregiverRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        IMedicationRepository medicationRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _caregiverRepo = caregiverRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _medicationRepo = medicationRepo;
        _uow = uow;
    }

    public async Task<Result> Handle(
        UpdateMedicationCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        // Resolve caregiver profile
        var caregiver = await _caregiverRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (caregiver is null)
            return Error.NotFound("caregiver.not_found", "Caregiver profile not found");

        // Resolve linked patients — collect all active patient ids
        var links = await _patientCaregiverRepo.GetActiveByCaregiverAsync(caregiver.Id, cancellationToken);
        if (!links.Any())
            return Error.NotFound("caregiver.no_patient", "Caregiver has no linked patient");

        var linkedPatientIds = links.Select(l => l.Patient.Id).ToHashSet();

        // Load medication and verify ownership
        var medication = await _medicationRepo.GetByIdAsync(request.MedicationId, cancellationToken);
        if (medication is null || !linkedPatientIds.Contains(medication.PatientId))
            return Error.NotFound("medication.not_found", "Medication not found");

        // Parse dates
        var startDate = DateOnly.ParseExact(request.StartDate, "yyyy-MM-dd");

        DateOnly? endDate = request.EndDate is not null
            ? DateOnly.ParseExact(request.EndDate, "yyyy-MM-dd")
            : null;

        // Apply domain update
        medication.UpdateDetails(
            name: request.Name,
            dose: request.Dose,
            frequency: request.Frequency,
            startDate: startDate,
            endDate: endDate,
            prescribingDoctorName: request.PrescribingDoctorName,
            notes: request.Notes);

        _medicationRepo.Update(medication);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}

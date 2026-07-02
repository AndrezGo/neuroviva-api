using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Medications;
using NeuroViva.Domain.Medications.Repositories;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Commands.CreateMedication;

public sealed class CreateMedicationCommandHandler
    : IRequestHandler<CreateMedicationCommand, Result<CreateMedicationResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IMedicationRepository _medicationRepo;
    private readonly IUnitOfWork _uow;

    public CreateMedicationCommandHandler(
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

    public async Task<Result<CreateMedicationResult>> Handle(
        CreateMedicationCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        // Resolve caregiver profile
        var caregiver = await _caregiverRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (caregiver is null)
            return Error.NotFound("caregiver.not_found", "Caregiver profile not found");

        // Resolve linked patient — take first active link (most recent by start_date)
        var links = await _patientCaregiverRepo.GetActiveByCaregiverAsync(caregiver.Id, cancellationToken);
        var link = links.FirstOrDefault();
        if (link is null)
            return Error.NotFound("caregiver.no_patient", "Caregiver has no linked patient");

        // Parse dates — startDate defaults to today if not provided
        var startDate = request.StartDate is not null
            ? DateOnly.ParseExact(request.StartDate, "yyyy-MM-dd")
            : DateOnly.FromDateTime(DateTime.UtcNow);

        DateOnly? endDate = request.EndDate is not null
            ? DateOnly.ParseExact(request.EndDate, "yyyy-MM-dd")
            : null;

        var medication = Medication.Prescribe(
            patientId: link.Patient.Id,
            name: request.Name,
            dose: request.Dose,
            frequency: request.Frequency,
            startDate: startDate,
            endDate: endDate,
            intervalHours: request.IntervalHours);

        await _medicationRepo.AddAsync(medication, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new CreateMedicationResult(medication.Id);
    }
}

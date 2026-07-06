using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.HealthMonitoring.Repositories;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Commands.DeleteSymptom;

public sealed class DeleteSymptomCommandHandler
    : IRequestHandler<DeleteSymptomCommand, Result>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly ISymptomRepository _symptomRepo;
    private readonly IUnitOfWork _uow;

    public DeleteSymptomCommandHandler(
        ICurrentUserService currentUser,
        ICaregiverRepository caregiverRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        ISymptomRepository symptomRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _caregiverRepo = caregiverRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _symptomRepo = symptomRepo;
        _uow = uow;
    }

    public async Task<Result> Handle(
        DeleteSymptomCommand request,
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

        // Load symptom and verify ownership.
        // Return NotFound (not Forbidden) to avoid leaking existence of other patients' symptoms.
        var symptom = await _symptomRepo.GetByIdAsync(request.SymptomId, cancellationToken);
        if (symptom is null || !linkedPatientIds.Contains(symptom.PatientId))
            return Error.NotFound("symptom.not_found", "Symptom not found");

        // Idempotent: Delete sets IsDeleted = true; calling it when already deleted is safe
        symptom.Delete();

        _symptomRepo.Update(symptom);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}

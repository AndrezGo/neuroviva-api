using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Catalog.Repositories;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Commands.SaveOnboarding;

public sealed class SaveCaregiverOnboardingCommandHandler
    : IRequestHandler<SaveCaregiverOnboardingCommand, Result<SaveCaregiverOnboardingResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IDiseaseRepository _diseaseRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IUnitOfWork _uow;

    public SaveCaregiverOnboardingCommandHandler(
        ICurrentUserService currentUser,
        ICaregiverRepository caregiverRepo,
        IDiseaseRepository diseaseRepo,
        IPatientRepository patientRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _caregiverRepo = caregiverRepo;
        _diseaseRepo = diseaseRepo;
        _patientRepo = patientRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _uow = uow;
    }

    public async Task<Result<SaveCaregiverOnboardingResult>> Handle(
        SaveCaregiverOnboardingCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve current user identity
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (_currentUser.TenantId is null)
            return Error.Unauthorized("Tenant not resolved for current user.");

        var userId = _currentUser.UserId.Value;
        var tenantId = _currentUser.TenantId.Value;

        // 2. Find or create the Caregiver row
        var caregiver = await _caregiverRepo.GetByUserIdAsync(userId, cancellationToken);
        if (caregiver is null)
        {
            caregiver = Caregiver.Create(userId, request.Relation);
            await _caregiverRepo.AddAsync(caregiver, cancellationToken);
        }
        else if (string.IsNullOrWhiteSpace(caregiver.PatientRelationship) &&
                 !string.IsNullOrWhiteSpace(request.Relation))
        {
            caregiver.SetRelationship(request.Relation);
            _caregiverRepo.Update(caregiver);
        }

        // 3. Resolve Disease (slug lookup → name fallback → NotFound)
        var slug = request.Condition.Trim().ToLowerInvariant();
        var disease = await _diseaseRepo.GetBySlugAsync(slug, cancellationToken)
                      ?? await _diseaseRepo.GetByNameAsync(request.Condition.Trim(), cancellationToken);

        if (disease is null)
            return Error.NotFound("disease.not_found", $"Disease '{request.Condition}' not found");

        // 4. Compute date_of_birth from age if provided
        DateOnly? dateOfBirth = request.PatientAge.HasValue
            ? DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-request.PatientAge.Value)
            : null;

        // 5. Look up existing active patient_caregiver rows for this caregiver
        // We must save the caregiver row before querying by its id, in case it was just created.
        await _uow.SaveChangesAsync(cancellationToken);

        var existingLinks = await _patientCaregiverRepo.GetActiveByCaregiverAsync(
            caregiver.Id, cancellationToken);

        Guid patientId;

        if (existingLinks.Count > 0)
        {
            // Update the most-recent active patient
            var link = existingLinks[0];
            var patient = link.Patient;

            patient.UpdateOnboardingInfo(request.PatientName, disease.Id, dateOfBirth);
            _patientRepo.Update(patient);

            link.Link.UpdateCareRole(request.Relation);
            _patientCaregiverRepo.Update(link.Link);

            patientId = patient.Id;
        }
        else
        {
            // Create a new patient and link
            var patient = Patient.Create(
                tenantId: tenantId,
                name: request.PatientName,
                diseaseId: disease.Id,
                dateOfBirth: dateOfBirth);

            await _patientRepo.AddAsync(patient, cancellationToken);

            var link = PatientCaregiver.Assign(
                patientId: patient.Id,
                caregiverId: caregiver.Id,
                careRole: request.Relation);

            await _patientCaregiverRepo.AddAsync(link, cancellationToken);

            patientId = patient.Id;
        }

        await _uow.SaveChangesAsync(cancellationToken);

        return new SaveCaregiverOnboardingResult(patientId);
    }
}

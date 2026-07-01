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

        // 3. Resolve Disease (slug lookup → name fallback → null if catalog not seeded yet)
        var slug = request.Condition.Trim().ToLowerInvariant();
        var disease = await _diseaseRepo.GetBySlugAsync(slug, cancellationToken)
                      ?? await _diseaseRepo.GetByNameAsync(request.Condition.Trim(), cancellationToken);
        // disease may be null when the catalog table is not yet seeded;
        // the patient is still created — diseaseId gets populated later.

        // 4. Resolve date_of_birth — prefer explicit DOB; fall back to age-derived for retro-compat; null otherwise
        DateOnly? dateOfBirth = request.PatientDateOfBirth
            ?? (request.PatientAge.HasValue
                ? DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-request.PatientAge.Value)
                : null);

        // 5. Persist the caregiver before the patient lookup so its Id is stable.
        await _uow.SaveChangesAsync(cancellationToken);

        // 6. Find-or-create patient by document number within this tenant.
        var patient = await _patientRepo.GetByDocumentNumberAsync(
            tenantId, request.DocumentNumber, cancellationToken);

        if (patient is null)
        {
            patient = Patient.Create(
                tenantId: tenantId,
                name: request.PatientName,
                documentNumber: request.DocumentNumber,
                diseaseId: disease?.Id,
                dateOfBirth: dateOfBirth);

            await _patientRepo.AddAsync(patient, cancellationToken);
        }
        else
        {
            // Only update profile data if the patient has not yet claimed their account.
            var updated = patient.UpdateProfileIfUnclaimed(request.PatientName, disease?.Id, dateOfBirth);
            if (updated)
                _patientRepo.Update(patient);
        }

        // 7. Find or create the patient-caregiver link to avoid duplicates.
        // The caregiver was persisted above, so caregiver.Id is guaranteed to be in the DB.
        // Patient may be new (not yet persisted), but we use a second SaveChanges below.
        // We must persist the patient first so the FK is resolvable for the link check.
        await _uow.SaveChangesAsync(cancellationToken);

        var existingLink = await _patientCaregiverRepo.GetByPatientAndCaregiverAsync(
            patient.Id, caregiver.Id, cancellationToken);

        if (existingLink is null)
        {
            var link = PatientCaregiver.Assign(
                patientId: patient.Id,
                caregiverId: caregiver.Id,
                careRole: request.Relation);

            await _patientCaregiverRepo.AddAsync(link, cancellationToken);
        }
        else
        {
            existingLink.UpdateCareRole(request.Relation);
            _patientCaregiverRepo.Update(existingLink);
        }

        await _uow.SaveChangesAsync(cancellationToken);

        return new SaveCaregiverOnboardingResult(patient.Id);
    }
}

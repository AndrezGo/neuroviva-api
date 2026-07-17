using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Authorization;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Common.Services;

public sealed class PatientAccessGuard : IPatientAccessGuard
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IPatientDoctorRepository _patientDoctorRepo;

    public PatientAccessGuard(
        ICurrentUserService currentUser,
        ICaregiverRepository caregiverRepo,
        IDoctorRepository doctorRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        IPatientDoctorRepository patientDoctorRepo)
    {
        _currentUser = currentUser;
        _caregiverRepo = caregiverRepo;
        _doctorRepo = doctorRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _patientDoctorRepo = patientDoctorRepo;
    }

    public async Task<Result<Guid>> ResolveAndAuthorizeAsync(Guid? requestedPatientId, CancellationToken ct)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced.");

        if (_currentUser.IsInRole(Roles.Doctor))
        {
            if (requestedPatientId is null)
                return Error.Validation("access.patient_required", "patientId is required for doctor callers.");

            var doctor = await _doctorRepo.GetByUserIdAsync(_currentUser.UserId.Value, ct);
            if (doctor is null)
                return Error.NotFound("doctor.not_found", "Doctor profile not found.");

            var link = await _patientDoctorRepo.GetByPatientAndDoctorAsync(requestedPatientId.Value, doctor.Id, ct);
            if (link is null || !link.IsActive)
                return Error.Forbidden("Doctor is not actively linked to this patient.");

            return Result<Guid>.Success(requestedPatientId.Value);
        }

        if (_currentUser.IsInRole(Roles.Caregiver))
        {
            var caregiver = await _caregiverRepo.GetByUserIdAsync(_currentUser.UserId.Value, ct);
            if (caregiver is null)
                return Error.NotFound("caregiver.not_found", "Caregiver profile not found.");

            if (requestedPatientId is null)
            {
                var links = await _patientCaregiverRepo.GetActiveByCaregiverAsync(caregiver.Id, ct);
                var link = links.FirstOrDefault();
                if (link is null)
                    return Error.NotFound("caregiver.no_patient", "Caregiver has no linked patient.");

                return Result<Guid>.Success(link.Patient.Id);
            }
            else
            {
                var link = await _patientCaregiverRepo.GetByPatientAndCaregiverAsync(requestedPatientId.Value, caregiver.Id, ct);
                if (link is null)
                    return Error.Forbidden("Caregiver is not linked to this patient.");

                return Result<Guid>.Success(requestedPatientId.Value);
            }
        }

        return Error.Forbidden("Only caregivers and doctors can access patient records.");
    }
}

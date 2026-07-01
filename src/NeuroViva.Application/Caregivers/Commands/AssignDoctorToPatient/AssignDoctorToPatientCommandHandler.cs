using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Commands.AssignDoctorToPatient;

public sealed class AssignDoctorToPatientCommandHandler : IRequestHandler<AssignDoctorToPatientCommand, Result>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IPatientDoctorRepository _patientDoctorRepo;
    private readonly IUnitOfWork _uow;

    public AssignDoctorToPatientCommandHandler(
        ICurrentUserService currentUser,
        ICaregiverRepository caregiverRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        IDoctorRepository doctorRepo,
        IPatientDoctorRepository patientDoctorRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _caregiverRepo = caregiverRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _doctorRepo = doctorRepo;
        _patientDoctorRepo = patientDoctorRepo;
        _uow = uow;
    }

    public async Task<Result> Handle(AssignDoctorToPatientCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var caregiver = await _caregiverRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (caregiver is null)
            return Error.NotFound("caregiver.not_found", "Caregiver profile not found");

        var links = await _patientCaregiverRepo.GetActiveByCaregiverAsync(caregiver.Id, cancellationToken);
        var link = links.FirstOrDefault();
        if (link is null)
            return Error.NotFound("caregiver.no_patient", "Caregiver has no linked patient");

        var doctor = await _doctorRepo.GetByIdAsync(request.DoctorId, cancellationToken);
        if (doctor is null)
            return Error.NotFound("doctor.not_found", "Doctor not found");

        // Check current active link
        var currentActive = await _patientDoctorRepo.GetActiveByPatientAsync(link.Patient.Id, cancellationToken);

        // Idempotency: same doctor already active → no-op
        if (currentActive is not null && currentActive.DoctorId == request.DoctorId)
            return Result.Ok;

        // Deactivate the current active link (if it belongs to a different doctor)
        if (currentActive is not null)
        {
            currentActive.Deactivate();
            _patientDoctorRepo.Update(currentActive);
        }

        // Reuse history row if (patient, newDoctor) ever existed; otherwise create new
        var historical = await _patientDoctorRepo.GetByPatientAndDoctorAsync(
            link.Patient.Id, request.DoctorId, cancellationToken);

        if (historical is not null)
        {
            historical.Reactivate();
            _patientDoctorRepo.Update(historical);
        }
        else
        {
            var newLink = PatientDoctor.Assign(link.Patient.Id, request.DoctorId);
            await _patientDoctorRepo.AddAsync(newLink, cancellationToken);
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Ok;
    }
}

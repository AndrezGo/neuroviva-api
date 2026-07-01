using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Doctors;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Queries.GetPatientDoctor;

public sealed class GetPatientDoctorQueryHandler
    : IRequestHandler<GetPatientDoctorQuery, Result<PatientDoctorDto?>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IDoctorReadRepository _doctorReadRepo;

    public GetPatientDoctorQueryHandler(
        ICurrentUserService currentUser,
        ICaregiverRepository caregiverRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        IDoctorReadRepository doctorReadRepo)
    {
        _currentUser = currentUser;
        _caregiverRepo = caregiverRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _doctorReadRepo = doctorReadRepo;
    }

    public async Task<Result<PatientDoctorDto?>> Handle(
        GetPatientDoctorQuery request,
        CancellationToken cancellationToken)
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

        var dto = await _doctorReadRepo.GetCurrentDoctorForPatientAsync(link.Patient.Id, cancellationToken);
        return Result<PatientDoctorDto?>.Success(dto);
    }
}

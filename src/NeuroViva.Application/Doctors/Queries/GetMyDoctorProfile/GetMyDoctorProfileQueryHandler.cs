using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Doctors.Queries.GetMyDoctorProfile;

public sealed class GetMyDoctorProfileQueryHandler
    : IRequestHandler<GetMyDoctorProfileQuery, Result<MyDoctorProfileDto?>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDoctorRepository _doctorRepository;

    public GetMyDoctorProfileQueryHandler(
        ICurrentUserService currentUser,
        IDoctorRepository doctorRepository)
    {
        _currentUser = currentUser;
        _doctorRepository = doctorRepository;
    }

    public async Task<Result<MyDoctorProfileDto?>> Handle(
        GetMyDoctorProfileQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var userId = _currentUser.UserId.Value;

        var doctor = await _doctorRepository.GetByUserIdAsync(userId, cancellationToken);

        if (doctor is null)
            return Error.NotFound(
                "doctor.profile_not_found",
                "No se encontró el perfil del médico.");

        return new MyDoctorProfileDto(
            doctor.Id,
            doctor.UserId,
            doctor.Specialty!,
            doctor.MedicalLicense!,
            doctor.IsScientificCommittee,
            doctor.CreatedAt);
    }
}

using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Doctors.Queries.GetDoctorPatients;

public sealed class GetDoctorPatientsQueryHandler
    : IRequestHandler<GetDoctorPatientsQuery, Result<DoctorPatientDto[]>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IDoctorReadRepository _readRepo;

    public GetDoctorPatientsQueryHandler(
        ICurrentUserService currentUser,
        IDoctorRepository doctorRepo,
        IDoctorReadRepository readRepo)
    {
        _currentUser = currentUser;
        _doctorRepo = doctorRepo;
        _readRepo = readRepo;
    }

    public async Task<Result<DoctorPatientDto[]>> Handle(
        GetDoctorPatientsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var doctor = await _doctorRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (doctor is null)
            return Error.NotFound("doctor.not_found", "Doctor profile not found");

        var dtos = await _readRepo.ListPatientsAsync(doctor.Id, cancellationToken);
        return Result<DoctorPatientDto[]>.Success(dtos.ToArray());
    }
}

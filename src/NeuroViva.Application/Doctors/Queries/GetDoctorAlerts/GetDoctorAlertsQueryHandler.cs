using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Doctors.Queries.GetDoctorAlerts;

public sealed class GetDoctorAlertsQueryHandler
    : IRequestHandler<GetDoctorAlertsQuery, Result<DoctorAlertDto[]>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IDoctorReadRepository _readRepo;

    public GetDoctorAlertsQueryHandler(
        ICurrentUserService currentUser,
        IDoctorRepository doctorRepo,
        IDoctorReadRepository readRepo)
    {
        _currentUser = currentUser;
        _doctorRepo = doctorRepo;
        _readRepo = readRepo;
    }

    public async Task<Result<DoctorAlertDto[]>> Handle(
        GetDoctorAlertsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var doctor = await _doctorRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (doctor is null)
            return Error.NotFound("doctor.not_found", "Doctor profile not found");

        var dtos = await _readRepo.ListAlertsAsync(doctor.Id, request.IncludeResolved, cancellationToken);
        return Result<DoctorAlertDto[]>.Success(dtos.ToArray());
    }
}

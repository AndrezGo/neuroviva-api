using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Doctors.Commands.ResolveAlert;

public sealed class ResolveAlertCommandHandler : IRequestHandler<ResolveAlertCommand, Result>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IAlertRepository _alertRepo;
    private readonly IUnitOfWork _uow;

    public ResolveAlertCommandHandler(
        ICurrentUserService currentUser,
        IDoctorRepository doctorRepo,
        IAlertRepository alertRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _doctorRepo = doctorRepo;
        _alertRepo = alertRepo;
        _uow = uow;
    }

    public async Task<Result> Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var doctor = await _doctorRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (doctor is null)
            return Error.NotFound("doctor.not_found", "Doctor profile not found");

        var alert = await _alertRepo.GetByIdAsync(request.AlertId, cancellationToken);
        if (alert is null)
            return Error.NotFound("alert.not_found", "Alert not found");

        if (alert.DoctorId != doctor.Id)
            return Error.Forbidden("Alert does not belong to current doctor");

        alert.Resolve();
        _alertRepo.Update(alert);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}

using MediatR;
using NeuroViva.Application.Caregivers;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetAppointments;

public sealed class GetAppointmentsQueryHandler
    : IRequestHandler<GetAppointmentsQuery, Result<IReadOnlyList<AppointmentListItemDto>>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverReadRepository _readRepo;

    public GetAppointmentsQueryHandler(
        ICurrentUserService currentUser,
        ICaregiverReadRepository readRepo)
    {
        _currentUser = currentUser;
        _readRepo = readRepo;
    }

    public async Task<Result<IReadOnlyList<AppointmentListItemDto>>> Handle(
        GetAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (_currentUser.TenantId is null)
            return Error.Unauthorized("Tenant not resolved for current user.");

        var appointments = await _readRepo.ListAppointmentsAsync(
            _currentUser.UserId.Value,
            _currentUser.TenantId.Value,
            cancellationToken);

        // No patient linked → return empty list (do NOT 404; matches GetToday pattern)
        return Result<IReadOnlyList<AppointmentListItemDto>>.Success(appointments);
    }
}

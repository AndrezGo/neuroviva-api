using MediatR;
using NeuroViva.Application.Caregivers;
using NeuroViva.Application.Caregivers.Queries.GetToday;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetToday;

public sealed class GetCaregiverTodayQueryHandler
    : IRequestHandler<GetCaregiverTodayQuery, Result<CaregiverTodayDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverReadRepository _readRepo;

    public GetCaregiverTodayQueryHandler(
        ICurrentUserService currentUser,
        ICaregiverReadRepository readRepo)
    {
        _currentUser = currentUser;
        _readRepo = readRepo;
    }

    public async Task<Result<CaregiverTodayDto>> Handle(
        GetCaregiverTodayQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (_currentUser.TenantId is null)
            return Error.Unauthorized("Tenant not resolved for current user.");

        var dto = await _readRepo.GetTodayAsync(
            _currentUser.UserId.Value,
            _currentUser.TenantId.Value,
            cancellationToken);

        // No patient → return empty arrays (do NOT 404; frontend treats empty as "no data")
        if (dto is null)
            return new CaregiverTodayDto(
                Medications: Array.Empty<TodayMedicationDto>(),
                Appointments: Array.Empty<TodayAppointmentDto>());

        return dto;
    }
}

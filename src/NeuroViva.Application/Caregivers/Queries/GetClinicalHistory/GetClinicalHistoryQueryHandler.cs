using MediatR;
using NeuroViva.Application.Caregivers;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetClinicalHistory;

public sealed class GetClinicalHistoryQueryHandler
    : IRequestHandler<GetClinicalHistoryQuery, Result<IReadOnlyList<HistoryEventDto>>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverReadRepository _readRepo;

    public GetClinicalHistoryQueryHandler(
        ICurrentUserService currentUser,
        ICaregiverReadRepository readRepo)
    {
        _currentUser = currentUser;
        _readRepo = readRepo;
    }

    public async Task<Result<IReadOnlyList<HistoryEventDto>>> Handle(
        GetClinicalHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (_currentUser.TenantId is null)
            return Error.Unauthorized("Tenant not resolved for current user.");

        var history = await _readRepo.ListClinicalHistoryAsync(
            _currentUser.UserId.Value,
            _currentUser.TenantId.Value,
            cancellationToken);

        return Result<IReadOnlyList<HistoryEventDto>>.Success(history);
    }
}

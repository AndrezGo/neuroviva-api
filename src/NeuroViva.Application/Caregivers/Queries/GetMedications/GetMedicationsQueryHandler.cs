using MediatR;
using NeuroViva.Application.Caregivers;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetMedications;

public sealed class GetMedicationsQueryHandler
    : IRequestHandler<GetMedicationsQuery, Result<IReadOnlyList<MedicationListItemDto>>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverReadRepository _readRepo;

    public GetMedicationsQueryHandler(
        ICurrentUserService currentUser,
        ICaregiverReadRepository readRepo)
    {
        _currentUser = currentUser;
        _readRepo = readRepo;
    }

    public async Task<Result<IReadOnlyList<MedicationListItemDto>>> Handle(
        GetMedicationsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (_currentUser.TenantId is null)
            return Error.Unauthorized("Tenant not resolved for current user.");

        var medications = await _readRepo.ListMedicationsAsync(
            _currentUser.UserId.Value,
            _currentUser.TenantId.Value,
            cancellationToken);

        // No patient linked → return empty list (do NOT 404; matches GetToday pattern)
        return Result<IReadOnlyList<MedicationListItemDto>>.Success(medications);
    }
}

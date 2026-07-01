using MediatR;
using NeuroViva.Application.Caregivers;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetSymptoms;

public sealed class GetSymptomsQueryHandler
    : IRequestHandler<GetSymptomsQuery, Result<IReadOnlyList<SymptomListItemDto>>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverReadRepository _readRepo;

    public GetSymptomsQueryHandler(
        ICurrentUserService currentUser,
        ICaregiverReadRepository readRepo)
    {
        _currentUser = currentUser;
        _readRepo = readRepo;
    }

    public async Task<Result<IReadOnlyList<SymptomListItemDto>>> Handle(
        GetSymptomsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (_currentUser.TenantId is null)
            return Error.Unauthorized("Tenant not resolved for current user.");

        var symptoms = await _readRepo.ListSymptomsAsync(
            _currentUser.UserId.Value,
            _currentUser.TenantId.Value,
            cancellationToken);

        return Result<IReadOnlyList<SymptomListItemDto>>.Success(symptoms);
    }
}

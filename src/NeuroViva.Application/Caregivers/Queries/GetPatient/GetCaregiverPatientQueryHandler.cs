using MediatR;
using NeuroViva.Application.Caregivers;
using NeuroViva.Application.Caregivers.Queries.GetPatient;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetPatient;

public sealed class GetCaregiverPatientQueryHandler
    : IRequestHandler<GetCaregiverPatientQuery, Result<CaregiverPatientDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverReadRepository _readRepo;

    public GetCaregiverPatientQueryHandler(
        ICurrentUserService currentUser,
        ICaregiverReadRepository readRepo)
    {
        _currentUser = currentUser;
        _readRepo = readRepo;
    }

    public async Task<Result<CaregiverPatientDto>> Handle(
        GetCaregiverPatientQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (_currentUser.TenantId is null)
            return Error.Unauthorized("Tenant not resolved for current user.");

        var dto = await _readRepo.GetActivePatientAsync(
            _currentUser.UserId.Value,
            _currentUser.TenantId.Value,
            cancellationToken);

        if (dto is null)
            // Distinguish between "no caregiver profile" and "no patient" via the repo returning null in both cases.
            // The repo returns null for both; the controller maps null → 404 with a generic message.
            // For richer messages the repo would need to signal the cause, but the contract only requires 404.
            return Error.NotFound("caregiver.patient_not_found", "No patient linked to this caregiver");

        return dto;
    }
}

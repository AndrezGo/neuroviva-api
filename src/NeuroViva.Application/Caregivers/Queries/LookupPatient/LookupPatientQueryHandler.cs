using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Patients.Repositories;

namespace NeuroViva.Application.Caregivers.Queries.LookupPatient;

public sealed class LookupPatientQueryHandler
    : IRequestHandler<LookupPatientQuery, Result<LookupPatientDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPatientRepository _patientRepo;

    public LookupPatientQueryHandler(
        ICurrentUserService currentUser,
        IPatientRepository patientRepo)
    {
        _currentUser = currentUser;
        _patientRepo = patientRepo;
    }

    public async Task<Result<LookupPatientDto>> Handle(
        LookupPatientQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (_currentUser.TenantId is null)
            return Error.Unauthorized("Tenant not resolved for current user.");

        var tenantId = _currentUser.TenantId.Value;

        var patient = await _patientRepo.GetByDocumentNumberAsync(
            tenantId,
            request.DocumentNumber,
            cancellationToken);

        if (patient is null)
            return Error.NotFound("patient.not_found", "Patient not found");

        return new LookupPatientDto(
            Id: patient.Id,
            Name: patient.Name,
            DocumentNumber: patient.DocumentNumber,
            HasUserAccount: patient.UserId.HasValue);
    }
}

using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Exceptions;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Patients.Commands.ClaimPatientProfile;

public sealed class ClaimPatientProfileCommandHandler
    : IRequestHandler<ClaimPatientProfileCommand, Result<ClaimPatientProfileResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPatientRepository _patientRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;

    public ClaimPatientProfileCommandHandler(
        ICurrentUserService currentUser,
        IPatientRepository patientRepo,
        IUserRepository userRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _patientRepo = patientRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<Result<ClaimPatientProfileResult>> Handle(
        ClaimPatientProfileCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (_currentUser.TenantId is null)
            return Error.Unauthorized("Tenant not resolved for current user.");

        var currentUserId = _currentUser.UserId.Value;
        var tenantId = _currentUser.TenantId.Value;

        var patient = await _patientRepo.FindClaimableByDocumentNumberAsync(
            request.DocumentNumber, currentUserId, cancellationToken);

        if (patient is null)
        {
            // Patient does not exist yet in any tenant — create it using the current user's name.
            var user = await _userRepo.GetByIdAsync(currentUserId, cancellationToken);
            var patientName = user?.Name ?? "Paciente";

            patient = Patient.Create(
                tenantId: tenantId,
                name: patientName,
                documentNumber: request.DocumentNumber,
                userId: currentUserId);

            await _patientRepo.AddAsync(patient, cancellationToken);
        }
        else
        {
            try
            {
                patient.LinkToUser(currentUserId);
            }
            catch (BusinessRuleViolationException ex) when (ex.RuleCode == "patient.already_claimed")
            {
                return Error.Conflict("patient.already_claimed", ex.Message);
            }

            _patientRepo.Update(patient);

            // If the patient lives in a different tenant (created by a caregiver), move the
            // current user into that tenant so subsequent requests resolve the correct care circle.
            if (patient.TenantId != tenantId)
            {
                var user = await _userRepo.GetByIdAsync(currentUserId, cancellationToken);

                if (user is null)
                    return Error.NotFound("user.not_found", "Current user not found.");

                user.MoveToTenant(patient.TenantId);
                _userRepo.Update(user);
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);

        return new ClaimPatientProfileResult(patient.Id);
    }
}

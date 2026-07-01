using MediatR;
using Microsoft.Extensions.Logging;
using NeuroViva.Application.Caregivers;
using NeuroViva.Application.Caregivers.Services;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Users.Repositories;
using NeuroViva.Domain.Patients.Repositories;

namespace NeuroViva.Application.Caregivers.Queries.GetAppointments;

public sealed class GetAppointmentsQueryHandler
    : IRequestHandler<GetAppointmentsQuery, Result<IReadOnlyList<AppointmentListItemDto>>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverReadRepository _readRepo;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IAppointmentReconciliationService _reconciliationService;
    private readonly ILogger<GetAppointmentsQueryHandler> _logger;

    public GetAppointmentsQueryHandler(
        ICurrentUserService currentUser,
        ICaregiverReadRepository readRepo,
        ICaregiverRepository caregiverRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        IAppointmentReconciliationService reconciliationService,
        ILogger<GetAppointmentsQueryHandler> logger)
    {
        _currentUser = currentUser;
        _readRepo = readRepo;
        _caregiverRepo = caregiverRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _reconciliationService = reconciliationService;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<AppointmentListItemDto>>> Handle(
        GetAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (_currentUser.TenantId is null)
            return Error.Unauthorized("Tenant not resolved for current user.");

        // Attempt reconciliation of missed appointments before returning the list.
        // Best-effort: errors are logged but do not fail the query.
        await TryReconcileAsync(cancellationToken);

        var appointments = await _readRepo.ListAppointmentsAsync(
            _currentUser.UserId.Value,
            _currentUser.TenantId.Value,
            cancellationToken);

        // No patient linked → return empty list (do NOT 404; matches GetToday pattern)
        return Result<IReadOnlyList<AppointmentListItemDto>>.Success(appointments);
    }

    private async Task TryReconcileAsync(CancellationToken ct)
    {
        try
        {
            if (_currentUser.UserId is null) return;

            var caregiver = await _caregiverRepo.GetByUserIdAsync(_currentUser.UserId.Value, ct);
            if (caregiver is null) return;

            var links = await _patientCaregiverRepo.GetActiveByCaregiverAsync(caregiver.Id, ct);
            var link = links.FirstOrDefault();
            if (link is null) return;

            await _reconciliationService.ReconcileForPatientAsync(link.Patient.Id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reconcile missed appointments during GetAppointments.");
        }
    }
}

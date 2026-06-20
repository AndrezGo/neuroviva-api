using MediatR;
using NeuroViva.Application.Caregivers;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Medications.Repositories;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Queries.GetMedicationLogs;

public sealed class GetMedicationLogsQueryHandler
    : IRequestHandler<GetMedicationLogsQuery, Result<IReadOnlyList<MedicationLogItemDto>>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IMedicationRepository _medicationRepo;
    private readonly ICaregiverReadRepository _readRepo;

    public GetMedicationLogsQueryHandler(
        ICurrentUserService currentUser,
        ICaregiverRepository caregiverRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        IMedicationRepository medicationRepo,
        ICaregiverReadRepository readRepo)
    {
        _currentUser = currentUser;
        _caregiverRepo = caregiverRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _medicationRepo = medicationRepo;
        _readRepo = readRepo;
    }

    public async Task<Result<IReadOnlyList<MedicationLogItemDto>>> Handle(
        GetMedicationLogsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (_currentUser.TenantId is null)
            return Error.Unauthorized("Tenant not resolved for current user.");

        // Resolve caregiver profile
        var caregiver = await _caregiverRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (caregiver is null)
            return Error.NotFound("caregiver.not_found", "Caregiver profile not found");

        // Resolve medication — 404 if not found
        var medication = await _medicationRepo.GetByIdAsync(request.MedicationId, cancellationToken);
        if (medication is null)
            return Error.NotFound("medication.not_found", "Medication not found");

        // 403 if the medication does not belong to any patient linked to this caregiver
        var links = await _patientCaregiverRepo.GetActiveByCaregiverAsync(caregiver.Id, cancellationToken);
        var isLinked = links.Any(l => l.Patient.Id == medication.PatientId);

        if (!isLinked)
            return Error.Forbidden("Medication does not belong to a patient linked to this caregiver");

        var logs = await _readRepo.ListMedicationLogsAsync(
            _currentUser.UserId.Value,
            _currentUser.TenantId.Value,
            request.MedicationId,
            cancellationToken);

        return Result<IReadOnlyList<MedicationLogItemDto>>.Success(logs);
    }
}

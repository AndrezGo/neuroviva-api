using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Catalog.Repositories;
using NeuroViva.Domain.Patients.Repositories;

namespace NeuroViva.Application.Patients.Queries.GetProfile;

public sealed class GetPatientProfileQueryHandler
    : IRequestHandler<GetPatientProfileQuery, Result<PatientProfileDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPatientRepository _patientRepo;
    private readonly IDiseaseRepository _diseaseRepo;

    public GetPatientProfileQueryHandler(
        ICurrentUserService currentUser,
        IPatientRepository patientRepo,
        IDiseaseRepository diseaseRepo)
    {
        _currentUser = currentUser;
        _patientRepo = patientRepo;
        _diseaseRepo = diseaseRepo;
    }

    public async Task<Result<PatientProfileDto>> Handle(
        GetPatientProfileQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var currentUserId = _currentUser.UserId.Value;

        var patient = await _patientRepo.GetByUserIdAsync(currentUserId, cancellationToken);

        if (patient is null)
            return Error.NotFound("patient.profile_not_found", "No patient profile linked to this user");

        string? conditionName = null;
        if (patient.DiseaseId.HasValue)
        {
            var disease = await _diseaseRepo.GetByIdAsync(patient.DiseaseId.Value, cancellationToken);
            conditionName = disease?.Name;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = patient.DateOfBirth.HasValue
            ? CalculateAge(patient.DateOfBirth.Value, today)
            : 0;

        return new PatientProfileDto(
            Id: patient.Id,
            Name: patient.Name,
            DocumentNumber: patient.DocumentNumber,
            Age: age,
            Condition: conditionName,
            DateOfBirth: patient.DateOfBirth);
    }

    private static int CalculateAge(DateOnly dateOfBirth, DateOnly today)
    {
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
}

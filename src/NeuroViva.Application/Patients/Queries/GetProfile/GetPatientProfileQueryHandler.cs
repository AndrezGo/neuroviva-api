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
    private readonly IPatientDiseaseRepository _patientDiseaseRepo;

    public GetPatientProfileQueryHandler(
        ICurrentUserService currentUser,
        IPatientRepository patientRepo,
        IDiseaseRepository diseaseRepo,
        IPatientDiseaseRepository patientDiseaseRepo)
    {
        _currentUser = currentUser;
        _patientRepo = patientRepo;
        _diseaseRepo = diseaseRepo;
        _patientDiseaseRepo = patientDiseaseRepo;
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

        var patientDiseases = await _patientDiseaseRepo.ListByPatientAsync(patient.Id, cancellationToken);
        var conditionNames = new List<string>();
        foreach (var pd in patientDiseases)
        {
            var disease = await _diseaseRepo.GetByIdAsync(pd.DiseaseId, cancellationToken);
            if (disease is not null)
                conditionNames.Add(disease.Name);
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
            Conditions: conditionNames,
            DateOfBirth: patient.DateOfBirth);
    }

    private static int CalculateAge(DateOnly dateOfBirth, DateOnly today)
    {
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
}

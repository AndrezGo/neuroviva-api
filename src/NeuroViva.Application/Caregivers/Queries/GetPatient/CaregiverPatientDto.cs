namespace NeuroViva.Application.Caregivers.Queries.GetPatient;

public sealed record CaregiverPatientDto(
    Guid Id,
    string Name,
    int Age,
    string Condition,
    string? ConditionStage
);

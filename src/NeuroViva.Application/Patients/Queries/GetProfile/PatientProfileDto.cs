namespace NeuroViva.Application.Patients.Queries.GetProfile;

public sealed record PatientProfileDto(
    Guid Id,
    string Name,
    string DocumentNumber,
    int Age,
    string? Condition,
    DateOnly? DateOfBirth);

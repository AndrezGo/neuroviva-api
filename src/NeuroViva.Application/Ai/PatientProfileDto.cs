namespace NeuroViva.Application.Ai;

public sealed record PatientProfileDto(
    string Name,
    int Age,
    string[] Conditions,
    Guid TenantId);

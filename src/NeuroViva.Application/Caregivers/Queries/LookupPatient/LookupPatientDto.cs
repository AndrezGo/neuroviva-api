namespace NeuroViva.Application.Caregivers.Queries.LookupPatient;

public sealed record LookupPatientDto(
    Guid Id,
    string Name,
    string DocumentNumber,
    bool HasUserAccount);

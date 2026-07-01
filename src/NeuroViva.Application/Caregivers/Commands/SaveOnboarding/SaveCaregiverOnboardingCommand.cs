using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.SaveOnboarding;

public sealed record SaveCaregiverOnboardingCommand(
    string PatientName,
    int? PatientAge,
    DateOnly? PatientDateOfBirth,
    string? Relation,
    string Condition,
    string DocumentNumber
) : IRequest<Result<SaveCaregiverOnboardingResult>>;

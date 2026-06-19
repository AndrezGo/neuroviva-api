using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.SaveOnboarding;

public sealed record SaveCaregiverOnboardingCommand(
    string PatientName,
    int? PatientAge,
    string? Relation,
    string Condition
) : IRequest<Result<SaveCaregiverOnboardingResult>>;

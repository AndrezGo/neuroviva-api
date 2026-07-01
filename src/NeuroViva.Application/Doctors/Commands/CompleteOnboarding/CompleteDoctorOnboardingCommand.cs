using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Doctors.Commands.CompleteOnboarding;

public sealed record CompleteDoctorOnboardingCommand(
    string Specialty,
    string MedicalLicense,
    string FirstName,
    string LastName
) : IRequest<Result<CompleteDoctorOnboardingResult>>;

using FluentValidation;

namespace NeuroViva.Application.Doctors.Commands.CompleteOnboarding;

public sealed class CompleteDoctorOnboardingCommandValidator
    : AbstractValidator<CompleteDoctorOnboardingCommand>
{
    public CompleteDoctorOnboardingCommandValidator()
    {
        RuleFor(x => x.Specialty)
            .NotEmpty()
            .Length(2, 100);

        RuleFor(x => x.MedicalLicense)
            .NotEmpty()
            .Length(4, 30)
            .Matches(@"^[A-Za-z0-9\-]+$")
            .WithMessage("MedicalLicense must contain only letters, digits and hyphens.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);
    }
}

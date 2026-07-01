using FluentValidation;

namespace NeuroViva.Application.Caregivers.Commands.SaveOnboarding;

public sealed class SaveCaregiverOnboardingCommandValidator : AbstractValidator<SaveCaregiverOnboardingCommand>
{
    public SaveCaregiverOnboardingCommandValidator()
    {
        RuleFor(x => x.PatientName)
            .NotEmpty()
            .Length(2, 200);

        RuleFor(x => x.PatientAge)
            .InclusiveBetween(0, 130)
            .When(x => x.PatientAge.HasValue);

        RuleFor(x => x.Relation)
            .MaximumLength(100)
            .When(x => x.Relation is not null);

        RuleFor(x => x.Conditions)
            .NotNull();

        RuleForEach(x => x.Conditions)
            .NotEmpty();

        RuleFor(x => x.DocumentNumber)
            .NotEmpty()
            .Length(5, 30)
            .Matches(@"^[A-Za-z0-9\-]+$")
            .WithMessage("DocumentNumber must contain only letters, digits and hyphens.");
    }
}

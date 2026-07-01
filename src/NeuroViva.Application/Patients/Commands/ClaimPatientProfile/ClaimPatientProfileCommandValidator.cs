using FluentValidation;

namespace NeuroViva.Application.Patients.Commands.ClaimPatientProfile;

public sealed class ClaimPatientProfileCommandValidator : AbstractValidator<ClaimPatientProfileCommand>
{
    public ClaimPatientProfileCommandValidator()
    {
        RuleFor(x => x.DocumentNumber)
            .NotEmpty()
            .Length(5, 30)
            .Matches(@"^[A-Za-z0-9\-]+$")
            .WithMessage("DocumentNumber must contain only letters, digits and hyphens.");
    }
}

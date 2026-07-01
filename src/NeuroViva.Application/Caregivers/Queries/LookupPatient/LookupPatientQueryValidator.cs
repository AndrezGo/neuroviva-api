using FluentValidation;

namespace NeuroViva.Application.Caregivers.Queries.LookupPatient;

public sealed class LookupPatientQueryValidator : AbstractValidator<LookupPatientQuery>
{
    public LookupPatientQueryValidator()
    {
        RuleFor(x => x.DocumentNumber)
            .NotEmpty()
            .Length(5, 30)
            .Matches(@"^[A-Za-z0-9\-]+$")
            .WithMessage("DocumentNumber must contain only letters, digits and hyphens.");
    }
}

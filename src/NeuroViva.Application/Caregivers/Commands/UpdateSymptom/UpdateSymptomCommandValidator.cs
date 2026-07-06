using FluentValidation;

namespace NeuroViva.Application.Caregivers.Commands.UpdateSymptom;

public sealed class UpdateSymptomCommandValidator : AbstractValidator<UpdateSymptomCommand>
{
    public UpdateSymptomCommandValidator()
    {
        RuleFor(x => x.SymptomId)
            .NotEmpty();

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Intensity)
            .InclusiveBetween(1, 10);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

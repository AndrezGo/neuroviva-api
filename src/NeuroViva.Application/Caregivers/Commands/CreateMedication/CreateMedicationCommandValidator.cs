using FluentValidation;

namespace NeuroViva.Application.Caregivers.Commands.CreateMedication;

public sealed class CreateMedicationCommandValidator : AbstractValidator<CreateMedicationCommand>
{
    public CreateMedicationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Dose)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Frequency)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.StartDate)
            .Must(BeValidDateOnly)
            .WithMessage("startDate must be a valid date in yyyy-MM-dd format.")
            .When(x => x.StartDate is not null);

        RuleFor(x => x.EndDate)
            .Must(BeValidDateOnly)
            .WithMessage("endDate must be a valid date in yyyy-MM-dd format.")
            .When(x => x.EndDate is not null);
    }

    private static bool BeValidDateOnly(string? value)
        => DateOnly.TryParseExact(value, "yyyy-MM-dd", out _);
}

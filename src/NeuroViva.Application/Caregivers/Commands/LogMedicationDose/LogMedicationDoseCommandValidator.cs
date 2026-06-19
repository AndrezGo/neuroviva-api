using FluentValidation;

namespace NeuroViva.Application.Caregivers.Commands.LogMedicationDose;

public sealed class LogMedicationDoseCommandValidator : AbstractValidator<LogMedicationDoseCommand>
{
    public LogMedicationDoseCommandValidator()
    {
        RuleFor(x => x.MedicationId).NotEmpty();

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes is not null);
    }
}

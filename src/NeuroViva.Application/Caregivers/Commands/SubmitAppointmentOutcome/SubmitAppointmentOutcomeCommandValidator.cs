using FluentValidation;

namespace NeuroViva.Application.Caregivers.Commands.SubmitAppointmentOutcome;

public sealed class SubmitAppointmentOutcomeCommandValidator
    : AbstractValidator<SubmitAppointmentOutcomeCommand>
{
    private static readonly string[] ValidOutcomes = { "attended", "missed", "cancelled" };

    public SubmitAppointmentOutcomeCommandValidator()
    {
        RuleFor(x => x.Outcome)
            .NotEmpty()
            .WithMessage("Outcome is required.")
            .Must(o => ValidOutcomes.Contains(o.ToLowerInvariant()))
            .WithMessage($"Outcome must be one of: {string.Join(", ", ValidOutcomes)}.");
    }
}

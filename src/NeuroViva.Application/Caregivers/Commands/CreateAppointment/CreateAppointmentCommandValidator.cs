using FluentValidation;

namespace NeuroViva.Application.Caregivers.Commands.CreateAppointment;

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ScheduledAt)
            .Must(BeValidDateTime)
            .WithMessage("scheduledAt must be a valid ISO 8601 datetime.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => x.Notes is not null);
    }

    private static bool BeValidDateTime(string value)
        => DateTime.TryParse(
            value,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
            out _);
}

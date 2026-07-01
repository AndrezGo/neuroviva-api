using FluentValidation;
using NeuroViva.Domain.HealthMonitoring.Enums;

namespace NeuroViva.Application.Caregivers.Commands.RegisterSymptom;

public sealed class RegisterSymptomCommandValidator : AbstractValidator<RegisterSymptomCommand>
{
    private static readonly HashSet<string> ValidTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        SymptomTypes.Agitation,
        SymptomTypes.Appetite,
        SymptomTypes.Memory,
        SymptomTypes.Sleep,
        SymptomTypes.Mobility,
        SymptomTypes.Pain,
        SymptomTypes.Confusion,
        SymptomTypes.Anxiety,
        SymptomTypes.Other
    };

    public RegisterSymptomCommandValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => ValidTypes.Contains(t))
            .WithMessage("type must be one of: agitacion, apetito, memoria, sueno, movilidad, dolor, confusion, ansiedad, otro.");

        RuleFor(x => x.Intensity)
            .InclusiveBetween(1, 10);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);

        RuleFor(x => x.LoggedAt)
            .Must(loggedAt => loggedAt!.Value <= DateTime.UtcNow.AddMinutes(5))
            .WithMessage("loggedAt cannot be in the future.")
            .When(x => x.LoggedAt is not null);
    }
}

using FluentValidation;

namespace NeuroViva.Application.Community.Commands.AddReaction;

public sealed class AddReactionCommandValidator : AbstractValidator<AddReactionCommand>
{
    private static readonly IReadOnlySet<string> AllowedTypes =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "apoyo",
            "animo",
            "gracias",
            "me_identifico",
            "fuerza"
        };

    public AddReactionCommandValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage("Reaction type must be one of: apoyo, animo, gracias, me_identifico, fuerza.");
    }
}

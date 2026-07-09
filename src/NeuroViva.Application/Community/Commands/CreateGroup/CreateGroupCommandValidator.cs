using FluentValidation;

namespace NeuroViva.Application.Community.Commands.CreateGroup;

public sealed class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    private static readonly IReadOnlySet<string> AllowedVisibilities =
        new HashSet<string>(StringComparer.Ordinal) { "public", "private" };

    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(120);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .Matches(@"^[a-z0-9-]{3,60}$")
            .WithMessage("Slug must be 3–60 characters and contain only lowercase letters, digits, or hyphens.");

        RuleFor(x => x.Visibility)
            .NotEmpty()
            .Must(v => AllowedVisibilities.Contains(v))
            .WithMessage("Visibility must be 'public' or 'private'.");
    }
}

using FluentValidation;

namespace NeuroViva.Application.Community.Commands.ModeratePost;

public sealed class ModeratePostCommandValidator : AbstractValidator<ModeratePostCommand>
{
    public ModeratePostCommandValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(500);
    }
}

using FluentValidation;

namespace NeuroViva.Application.Community.Commands.ModerateComment;

public sealed class ModerateCommentCommandValidator : AbstractValidator<ModerateCommentCommand>
{
    public ModerateCommentCommandValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(500);
    }
}

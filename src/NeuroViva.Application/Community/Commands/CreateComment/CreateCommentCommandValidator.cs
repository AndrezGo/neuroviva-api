using FluentValidation;

namespace NeuroViva.Application.Community.Commands.CreateComment;

public sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(2000);
    }
}

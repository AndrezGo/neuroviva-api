using FluentValidation;

namespace NeuroViva.Application.Features.Users.Commands;

public sealed class SyncUserCommandValidator : AbstractValidator<SyncUserCommand>
{
    public SyncUserCommandValidator()
    {
        RuleFor(x => x.AuthUserId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.TenantId).NotEmpty();
    }
}

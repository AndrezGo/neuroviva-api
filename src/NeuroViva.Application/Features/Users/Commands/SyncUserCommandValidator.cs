using FluentValidation;

namespace NeuroViva.Application.Features.Users.Commands;

public sealed class SyncUserCommandValidator : AbstractValidator<SyncUserCommand>
{
    public SyncUserCommandValidator()
    {
        RuleFor(x => x.AuthUserId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        // TenantId is optional — auto-creates a personal tenant when null
    }
}

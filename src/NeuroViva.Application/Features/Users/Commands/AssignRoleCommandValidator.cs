using FluentValidation;

namespace NeuroViva.Application.Features.Users.Commands;

public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    private static readonly string[] ValidRoles = ["paciente", "cuidador", "medico"];

    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleName)
            .NotEmpty()
            .Must(r => ValidRoles.Contains(r))
            .WithMessage("Invalid role name.");
    }
}

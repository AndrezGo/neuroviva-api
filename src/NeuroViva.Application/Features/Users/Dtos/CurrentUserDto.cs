namespace NeuroViva.Application.Features.Users.Dtos;

public sealed record CurrentUserDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Email,
    string? AvatarUrl,
    bool IsActive,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt
);

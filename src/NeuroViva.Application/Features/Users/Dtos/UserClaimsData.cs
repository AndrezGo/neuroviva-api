namespace NeuroViva.Application.Features.Users.Dtos;

public sealed record UserClaimsData(
    Guid InternalUserId,
    Guid TenantId,
    IReadOnlyList<string> Roles);

namespace NeuroViva.Application.Common.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? AuthUserId { get; }
    Guid? TenantId { get; }
    IReadOnlySet<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

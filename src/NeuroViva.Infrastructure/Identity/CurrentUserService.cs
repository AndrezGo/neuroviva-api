using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Authorization;

namespace NeuroViva.Infrastructure.Identity;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? AuthUserId
    {
        get
        {
            var sub = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User?.FindFirstValue(ClaimNames.Sub);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public Guid? UserId
    {
        get
        {
            var claim = User?.FindFirstValue(ClaimNames.InternalUserId);
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var claim = User?.FindFirstValue(ClaimNames.TenantId);
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public IReadOnlySet<string> Roles
    {
        get
        {
            return User?.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToHashSet() ?? new HashSet<string>();
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    public bool IsInRole(string role) => Roles.Contains(role);
}

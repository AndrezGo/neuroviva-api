using NeuroViva.Application.Common.Abstractions;

namespace NeuroViva.Infrastructure.Identity;

public sealed class TenantContext : ITenantContext
{
    private readonly ICurrentUserService _currentUser;

    public TenantContext(ICurrentUserService currentUser) => _currentUser = currentUser;

    public Guid? TenantId => _currentUser.TenantId;
}

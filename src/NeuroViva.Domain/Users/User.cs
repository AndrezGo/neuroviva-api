using NeuroViva.Domain.Common;
using NeuroViva.Domain.Exceptions;

namespace NeuroViva.Domain.Users;

public sealed class User : AggregateRoot<Guid>, ITenantOwned
{
    public Guid TenantId { get; private set; }
    public Guid? AuthUserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? AvatarUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<UserRole> _roles = new();
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    private User() { }

    public static User Create(Guid tenantId, string name, string email, Guid? authUserId = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AuthUserId = authUserId,
            Name = name,
            Email = email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void UpdateAvatar(string url) => AvatarUrl = url;
    public void UpdateName(string name) => Name = name;

    /// <summary>
    /// Moves this user to a different tenant. Used by the ClaimPatientProfile flow
    /// when a patient adopts a profile previously created by a caregiver in another tenant.
    /// Idempotent when the user is already in the target tenant.
    /// </summary>
    public void MoveToTenant(Guid newTenantId)
    {
        if (newTenantId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "user.invalid_tenant",
                "Tenant id cannot be empty.");

        if (TenantId == newTenantId)
            return;

        TenantId = newTenantId;
    }
}

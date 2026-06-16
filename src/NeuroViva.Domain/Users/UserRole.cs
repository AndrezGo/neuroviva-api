using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Users;

public sealed class UserRole : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    private UserRole() { }

    public static UserRole Assign(Guid userId, Guid roleId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        RoleId = roleId,
        AssignedAt = DateTime.UtcNow
    };
}

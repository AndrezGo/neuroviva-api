using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Users;

public sealed class Caregiver : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string? PatientRelationship { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Caregiver() { }

    public static Caregiver Create(Guid userId, string? relationship = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        PatientRelationship = relationship,
        CreatedAt = DateTime.UtcNow
    };

    public void SetRelationship(string? relationship) => PatientRelationship = relationship;
}

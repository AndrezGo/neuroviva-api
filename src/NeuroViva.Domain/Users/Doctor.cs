using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Users;

public sealed class Doctor : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string? Specialty { get; private set; }
    public string? MedicalLicense { get; private set; }
    public bool IsScientificCommittee { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Doctor() { }

    public static Doctor Create(Guid userId, string? specialty = null, string? medicalLicense = null)
    {
        return new Doctor
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Specialty = specialty,
            MedicalLicense = medicalLicense,
            IsScientificCommittee = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void GrantCommitteeAccess() => IsScientificCommittee = true;
    public void RevokeCommitteeAccess() => IsScientificCommittee = false;
}

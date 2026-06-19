using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Patients;

public sealed class PatientCaregiver : Entity<Guid>
{
    public Guid PatientId { get; private set; }
    public Guid CaregiverId { get; private set; }
    public string? CareRole { get; private set; }
    public DateOnly StartDate { get; private set; }

    private PatientCaregiver() { }

    public static PatientCaregiver Assign(Guid patientId, Guid caregiverId, string? careRole = null) => new()
    {
        Id = Guid.NewGuid(),
        PatientId = patientId,
        CaregiverId = caregiverId,
        CareRole = careRole,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
    };

    public void UpdateCareRole(string? careRole) => CareRole = careRole;
}

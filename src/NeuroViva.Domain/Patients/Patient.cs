using NeuroViva.Domain.Common;
using NeuroViva.Domain.Patients.Enums;
using NeuroViva.Domain.Patients.Events;

namespace NeuroViva.Domain.Patients;

public sealed class Patient : AggregateRoot<Guid>, ITenantOwned
{
    public Guid TenantId { get; private set; }
    public Guid? DiseaseId { get; private set; }
    public string Name { get; private set; } = default!;
    public DateOnly? DateOfBirth { get; private set; }
    public PatientStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<PatientDoctor> _doctors = new();
    public IReadOnlyCollection<PatientDoctor> Doctors => _doctors.AsReadOnly();

    private readonly List<PatientCaregiver> _caregivers = new();
    public IReadOnlyCollection<PatientCaregiver> Caregivers => _caregivers.AsReadOnly();

    private readonly List<ClinicalRecord> _clinicalRecords = new();
    public IReadOnlyCollection<ClinicalRecord> ClinicalRecords => _clinicalRecords.AsReadOnly();

    private Patient() { }

    public static Patient Create(Guid tenantId, string name, Guid? diseaseId = null, DateOnly? dateOfBirth = null)
    {
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            DiseaseId = diseaseId,
            DateOfBirth = dateOfBirth,
            Status = PatientStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        patient.RaiseEvent(new PatientCreatedDomainEvent(patient.Id, tenantId));
        return patient;
    }

    public void Deactivate() => Status = PatientStatus.Inactive;

    public void Discharge() => Status = PatientStatus.Discharged;
}

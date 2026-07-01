using NeuroViva.Domain.Common;
using NeuroViva.Domain.Exceptions;
using NeuroViva.Domain.Patients.Enums;
using NeuroViva.Domain.Patients.Events;

namespace NeuroViva.Domain.Patients;

public sealed class Patient : AggregateRoot<Guid>, ITenantOwned
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string DocumentNumber { get; private set; } = default!;
    public Guid? UserId { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public PatientStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<PatientDoctor> _doctors = new();
    public IReadOnlyCollection<PatientDoctor> Doctors => _doctors.AsReadOnly();

    private readonly List<PatientCaregiver> _caregivers = new();
    public IReadOnlyCollection<PatientCaregiver> Caregivers => _caregivers.AsReadOnly();

    private readonly List<ClinicalRecord> _clinicalRecords = new();
    public IReadOnlyCollection<ClinicalRecord> ClinicalRecords => _clinicalRecords.AsReadOnly();

    private readonly List<PatientDisease> _diseases = new();
    public IReadOnlyCollection<PatientDisease> Diseases => _diseases.AsReadOnly();

    public IReadOnlyCollection<Guid> DiseaseIds =>
        _diseases.Select(d => d.DiseaseId).ToList().AsReadOnly();

    private Patient() { }

    public static Patient Create(
        Guid tenantId,
        string name,
        string documentNumber,
        IEnumerable<Guid>? diseaseIds = null,
        DateOnly? dateOfBirth = null,
        Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("patient.name_required", "Patient name cannot be empty.");

        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new BusinessRuleViolationException("patient.document_number_required", "Patient document number cannot be empty.");

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.Trim(),
            DocumentNumber = documentNumber.Trim().ToUpperInvariant(),
            DateOfBirth = dateOfBirth,
            Status = PatientStatus.Active,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        if (diseaseIds is not null)
            patient.SetDiseases(diseaseIds);

        patient.RaiseEvent(new PatientCreatedDomainEvent(patient.Id, tenantId));
        return patient;
    }

    /// <summary>
    /// Links this patient to the specified user account.
    /// Idempotent if the patient is already linked to the same user.
    /// Throws <see cref="BusinessRuleViolationException"/> if already linked to a different user.
    /// </summary>
    public void LinkToUser(Guid userId)
    {
        if (UserId.HasValue && UserId.Value != userId)
            throw new BusinessRuleViolationException(
                "patient.already_claimed",
                "Patient already linked to another user");

        UserId = userId;
    }

    /// <summary>
    /// Updates name, diseases and date of birth only when the patient has not yet claimed
    /// their own account (UserId is null). Returns true if the update was applied.
    /// Empty collection is valid (means "no conditions specified").
    /// </summary>
    public bool UpdateProfileIfUnclaimed(string name, IEnumerable<Guid> diseaseIds, DateOnly? dateOfBirth)
    {
        if (UserId is not null)
            return false;

        Name = name;
        DateOfBirth = dateOfBirth;
        SetDiseases(diseaseIds);
        return true;
    }

    public void Deactivate() => Status = PatientStatus.Inactive;

    public void Discharge() => Status = PatientStatus.Discharged;

    private void SetDiseases(IEnumerable<Guid> diseaseIds)
    {
        _diseases.Clear();
        foreach (var diseaseId in diseaseIds.Distinct())
            _diseases.Add(PatientDisease.Assign(Id, diseaseId));
    }
}

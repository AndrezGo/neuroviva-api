using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Enums;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Commands.AddClinicalNote;

public sealed class AddClinicalNoteCommandHandler
    : IRequestHandler<AddClinicalNoteCommand, Result<AddClinicalNoteResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IClinicalRecordRepository _clinicalRecordRepo;
    private readonly IUnitOfWork _uow;

    public AddClinicalNoteCommandHandler(
        ICurrentUserService currentUser,
        ICaregiverRepository caregiverRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        IClinicalRecordRepository clinicalRecordRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _caregiverRepo = caregiverRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _clinicalRecordRepo = clinicalRecordRepo;
        _uow = uow;
    }

    public async Task<Result<AddClinicalNoteResult>> Handle(
        AddClinicalNoteCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (string.IsNullOrWhiteSpace(request.Description))
            return Error.Validation("clinical_note.description_required", "Description is required");

        // Resolve caregiver profile
        var caregiver = await _caregiverRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (caregiver is null)
            return Error.NotFound("caregiver.not_found", "Caregiver profile not found");

        // Resolve linked patient — take first active link (most recent by start_date)
        var links = await _patientCaregiverRepo.GetActiveByCaregiverAsync(caregiver.Id, cancellationToken);
        var link = links.FirstOrDefault();
        if (link is null)
            return Error.NotFound("caregiver.no_patient", "Caregiver has no linked patient");

        var eventType = MapEventType(request.EventType);

        var record = ClinicalRecord.Create(
            patientId: link.Patient.Id,
            createdBy: _currentUser.UserId.Value,
            eventType: eventType,
            description: request.Description.Trim(),
            eventDate: request.EventDate);

        await _clinicalRecordRepo.AddAsync(record, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new AddClinicalNoteResult(record.Id);
    }

    /// <summary>
    /// Maps a free-text event type (Spanish or English) to the ClinicalEventType enum.
    /// Defaults to Other when unrecognized.
    /// </summary>
    private static ClinicalEventType MapEventType(string raw) =>
        raw.Trim().ToLowerInvariant() switch
        {
            "consultation" or "consulta" => ClinicalEventType.Consultation,
            "exam" or "examen" => ClinicalEventType.Exam,
            "note" or "nota" => ClinicalEventType.Note,
            _ => ClinicalEventType.Other
        };
}

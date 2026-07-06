using System.Globalization;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuroViva.Application.Caregivers.Commands.AddClinicalNote;
using NeuroViva.Application.Caregivers.Commands.CancelAppointment;
using NeuroViva.Application.Caregivers.Commands.SubmitAppointmentOutcome;
using NeuroViva.Application.Caregivers.Commands.CreateAppointment;
using NeuroViva.Application.Caregivers.Commands.CreateMedication;
using NeuroViva.Application.Caregivers.Commands.DiscontinueMedication;
using NeuroViva.Application.Caregivers.Commands.LogMedicationDose;
using NeuroViva.Application.Caregivers.Commands.UpdateMedication;
using NeuroViva.Application.Caregivers.Commands.AssignDoctorToPatient;
using NeuroViva.Application.Caregivers.Commands.MarkNotificationRead;
using NeuroViva.Application.Caregivers.Commands.RegisterSymptom;
using NeuroViva.Application.Caregivers.Commands.UpdateSymptom;
using NeuroViva.Application.Caregivers.Commands.DeleteSymptom;
using NeuroViva.Application.Caregivers.Commands.SaveOnboarding;
using NeuroViva.Application.Caregivers.Queries.GetAppointments;
using NeuroViva.Application.Caregivers.Queries.GetPatientDoctor;
using NeuroViva.Application.Caregivers.Queries.GetClinicalHistory;
using NeuroViva.Application.Caregivers.Queries.GetMedicationLogs;
using NeuroViva.Application.Caregivers.Queries.GetMedications;
using NeuroViva.Application.Caregivers.Queries.GetNotifications;
using NeuroViva.Application.Caregivers.Queries.GetPatient;
using NeuroViva.Application.Caregivers.Queries.GetSymptoms;
using NeuroViva.Application.Caregivers.Queries.GetToday;
using NeuroViva.Application.Caregivers.Queries.LookupPatient;
using NeuroViva.Application.Common.Authorization;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/caregiver")]
[Authorize(Policy = Policies.CaregiverOnly)]
public sealed class CaregiverController : ControllerBase
{
    private readonly IMediator _mediator;

    public CaregiverController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Idempotent onboarding: finds or creates the caregiver profile, resolves (or creates)
    /// the linked patient, and associates the disease by condition name/slug.
    /// </summary>
    [HttpPost("onboarding")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveOnboarding(
        [FromBody] SaveOnboardingRequest request,
        CancellationToken ct)
    {
        DateOnly? patientDob = null;
        if (!string.IsNullOrWhiteSpace(request.PatientDateOfBirth))
        {
            if (!DateOnly.TryParseExact(
                    request.PatientDateOfBirth,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                return BadRequest("Invalid PatientDateOfBirth format. Expected YYYY-MM-DD.");
            }
            patientDob = parsed;
        }

        var command = new SaveCaregiverOnboardingCommand(
            PatientName: request.PatientName,
            PatientAge: request.PatientAge,
            PatientDateOfBirth: patientDob,
            Relation: request.Relation,
            Conditions: request.Conditions,
            DocumentNumber: request.DocumentNumber);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new { patientId = result.Value.PatientId });
    }

    /// <summary>
    /// Looks up a patient by document number within the caregiver's tenant.
    /// Returns basic info and whether the patient has already claimed their account.
    /// </summary>
    [HttpGet("patient/lookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupPatient(
        [FromQuery] string documentNumber,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new LookupPatientQuery(documentNumber), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns the active patient linked to the authenticated caregiver.
    /// </summary>
    [HttpGet("patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatient(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCaregiverPatientQuery(), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns today's medications and appointments for the caregiver's linked patient.
    /// Returns empty arrays when no patient is linked — never throws.
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToday(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCaregiverTodayQuery(), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Logs a taken dose for the specified medication.
    /// The medication must belong to the authenticated caregiver's linked patient.
    /// </summary>
    [HttpPost("medications/{medicationId:guid}/log")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LogMedicationDose(
        Guid medicationId,
        [FromBody] LogMedicationDoseRequest request,
        CancellationToken ct)
    {
        var command = new LogMedicationDoseCommand(
            MedicationId: medicationId,
            Notes: request.Notes);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new { logId = result.Value.LogId });
    }

    /// <summary>
    /// Returns the dose log history for the specified medication.
    /// The medication must belong to the authenticated caregiver's linked patient.
    /// </summary>
    [HttpGet("medications/{medicationId:guid}/logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMedicationLogs(
        Guid medicationId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMedicationLogsQuery(medicationId), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Lists all medications for the caregiver's linked patient.
    /// Returns an empty array when no patient is linked.
    /// </summary>
    [HttpGet("medications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMedications(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMedicationsQuery(), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a medication for the caregiver's linked patient.
    /// </summary>
    [HttpPost("medications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMedication(
        [FromBody] CreateMedicationRequest request,
        CancellationToken ct)
    {
        var command = new CreateMedicationCommand(
            Name: request.Name,
            Dose: request.Dose,
            Frequency: request.Frequency,
            StartDate: request.StartDate,
            EndDate: request.EndDate,
            PrescribingDoctorName: request.PrescribingDoctorName,
            Notes: request.Notes);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new { medicationId = result.Value.MedicationId });
    }

    /// <summary>
    /// Updates an existing medication for the caregiver's linked patient.
    /// The medication must belong to the authenticated caregiver's linked patient.
    /// </summary>
    [HttpPatch("medications/{medicationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMedication(
        Guid medicationId,
        [FromBody] UpdateMedicationRequest request,
        CancellationToken ct)
    {
        var command = new UpdateMedicationCommand(
            MedicationId: medicationId,
            Name: request.Name,
            Dose: request.Dose,
            Frequency: request.Frequency,
            StartDate: request.StartDate,
            EndDate: request.EndDate,
            PrescribingDoctorName: request.PrescribingDoctorName,
            Notes: request.Notes);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return NoContent();
    }

    /// <summary>
    /// Discontinues (soft-deletes) a medication for the caregiver's linked patient.
    /// Idempotent: succeeds even when the medication is already inactive.
    /// </summary>
    [HttpDelete("medications/{medicationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DiscontinueMedication(
        Guid medicationId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new DiscontinueMedicationCommand(medicationId), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return NoContent();
    }

    /// <summary>
    /// Lists appointments for the caregiver's linked patient (upcoming first).
    /// Returns an empty array when no patient is linked.
    /// </summary>
    [HttpGet("appointments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAppointments(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAppointmentsQuery(), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates an appointment for the caregiver's linked patient.
    /// </summary>
    [HttpPost("appointments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAppointment(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken ct)
    {
        var command = new CreateAppointmentCommand(
            Title: request.Title,
            Type: request.Type,
            ScheduledAt: request.ScheduledAt,
            Notes: request.Notes);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new { appointmentId = result.Value.AppointmentId });
    }

    /// <summary>
    /// Cancels a scheduled or confirmed appointment for the caregiver's linked patient.
    /// </summary>
    [HttpPatch("appointments/{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelAppointment(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelAppointmentCommand(id), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                ErrorType.Conflict => Conflict(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return NoContent();
    }

    /// <summary>
    /// Records the outcome of a past appointment: attended, missed, or cancelled.
    /// </summary>
    [HttpPatch("appointments/{id:guid}/outcome")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitAppointmentOutcome(
        Guid id,
        [FromBody] SubmitAppointmentOutcomeRequest request,
        CancellationToken ct)
    {
        var command = new SubmitAppointmentOutcomeCommand(id, request.Outcome);
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                ErrorType.Conflict => Conflict(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return NoContent();
    }

    /// <summary>
    /// Lists the most recent symptoms (top 50) for the caregiver's linked patient.
    /// Returns an empty array when no patient is linked.
    /// </summary>
    [HttpGet("symptoms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSymptoms(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSymptomsQuery(), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Registers a symptom for the caregiver's linked patient.
    /// </summary>
    [HttpPost("symptoms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterSymptom(
        [FromBody] RegisterSymptomRequest request,
        CancellationToken ct)
    {
        var command = new RegisterSymptomCommand(
            Type: request.Type,
            Intensity: request.Intensity,
            Description: request.Description,
            LoggedAt: request.LoggedAt);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new { symptomId = result.Value.SymptomId });
    }

    /// <summary>
    /// Updates an existing symptom for the caregiver's linked patient.
    /// The symptom must belong to the authenticated caregiver's linked patient.
    /// </summary>
    [HttpPatch("symptoms/{symptomId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSymptom(
        Guid symptomId,
        [FromBody] UpdateSymptomRequest request,
        CancellationToken ct)
    {
        var command = new UpdateSymptomCommand(
            SymptomId: symptomId,
            Type: request.Type,
            Intensity: request.Intensity,
            Description: request.Description);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return NoContent();
    }

    /// <summary>
    /// Soft-deletes a symptom for the caregiver's linked patient.
    /// Idempotent: succeeds even when the symptom is already deleted.
    /// </summary>
    [HttpDelete("symptoms/{symptomId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSymptom(
        Guid symptomId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteSymptomCommand(symptomId), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return NoContent();
    }

    /// <summary>
    /// Returns the unified clinical history timeline for the caregiver's linked patient.
    /// Combines symptoms, appointments, medication logs and manual clinical records.
    /// Returns an empty array when no patient is linked.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetClinicalHistoryQuery(), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Adds a manual clinical note (consultation, exam, note or other) to the patient's history.
    /// Optionally attach a scanned document or image (max 10 MB; accepted types: image/jpeg, image/png, image/webp, application/pdf).
    /// </summary>
    [HttpPost("history")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddClinicalNote(
        [FromForm] AddClinicalNoteRequest request,
        CancellationToken ct)
    {
        byte[]? attachmentBytes = null;
        string? attachmentFileName = null;
        string? attachmentContentType = null;

        if (request.Attachment is not null)
        {
            using var ms = new MemoryStream();
            await request.Attachment.CopyToAsync(ms, ct);
            attachmentBytes = ms.ToArray();
            attachmentFileName = request.Attachment.FileName;
            attachmentContentType = request.Attachment.ContentType;
        }

        var command = new AddClinicalNoteCommand(
            EventType: request.EventType,
            Description: request.Description,
            EventDate: request.EventDate,
            AttachmentBytes: attachmentBytes,
            AttachmentFileName: attachmentFileName,
            AttachmentContentType: attachmentContentType);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new { recordId = result.Value.RecordId });
    }

    /// <summary>
    /// Returns the most recent 30 InApp notifications for the authenticated caregiver.
    /// </summary>
    [HttpGet("notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetNotificationsQuery(), ct);
        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message),
            };
        return Ok(result.Value);
    }

    /// <summary>
    /// Marks an InApp notification as read for the authenticated caregiver.
    /// </summary>
    [HttpPatch("notifications/{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkNotificationRead(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new MarkNotificationReadCommand(id), ct);
        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound     => NotFound(result.Error.Message),
                ErrorType.Forbidden    => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message),
            };
        return NoContent();
    }

    /// <summary>
    /// Returns the doctor currently assigned to the caregiver's linked patient, or null if none.
    /// </summary>
    [HttpGet("patient/doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientDoctor(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPatientDoctorQuery(), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Assigns (or replaces) the active doctor for the caregiver's linked patient.
    /// Idempotent: if the same doctor is already active, succeeds without changes.
    /// </summary>
    [HttpPost("patient/doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignDoctorToPatient(
        [FromBody] AssignDoctorToPatientRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new AssignDoctorToPatientCommand(request.DoctorId), ct);
        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        return NoContent();
    }
}

public sealed record SaveOnboardingRequest(
    string PatientName,
    int? PatientAge,
    string? PatientDateOfBirth,
    string? Relation,
    IReadOnlyList<string> Conditions,
    string DocumentNumber);

public sealed record LogMedicationDoseRequest(string? Notes);

public sealed record CreateMedicationRequest(
    string Name,
    string Dose,
    string Frequency,
    string? StartDate,
    string? EndDate,
    string? PrescribingDoctorName,
    string? Notes);

public sealed record UpdateMedicationRequest(
    string Name,
    string Dose,
    string Frequency,
    string StartDate,
    string? EndDate,
    string? PrescribingDoctorName,
    string? Notes);

public sealed record CreateAppointmentRequest(
    string Title,
    string Type,
    string ScheduledAt,
    string? Notes);

public sealed record RegisterSymptomRequest(
    string Type,
    int Intensity,
    string? Description,
    DateTime? LoggedAt);

public sealed record UpdateSymptomRequest(string Type, int Intensity, string? Description);

public sealed class AddClinicalNoteRequest
{
    public string EventType { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime? EventDate { get; set; }
    public IFormFile? Attachment { get; set; }
}

public sealed record AssignDoctorToPatientRequest(Guid DoctorId);

public sealed record SubmitAppointmentOutcomeRequest(string Outcome);

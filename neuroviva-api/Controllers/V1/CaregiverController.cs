using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuroViva.Application.Caregivers.Commands.CreateAppointment;
using NeuroViva.Application.Caregivers.Commands.CreateMedication;
using NeuroViva.Application.Caregivers.Commands.LogMedicationDose;
using NeuroViva.Application.Caregivers.Commands.SaveOnboarding;
using NeuroViva.Application.Caregivers.Queries.GetAppointments;
using NeuroViva.Application.Caregivers.Queries.GetMedicationLogs;
using NeuroViva.Application.Caregivers.Queries.GetMedications;
using NeuroViva.Application.Caregivers.Queries.GetPatient;
using NeuroViva.Application.Caregivers.Queries.GetToday;
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
        var command = new SaveCaregiverOnboardingCommand(
            PatientName: request.PatientName,
            PatientAge: request.PatientAge,
            Relation: request.Relation,
            Condition: request.Condition);

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
            EndDate: request.EndDate);

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
}

public sealed record SaveOnboardingRequest(
    string PatientName,
    int? PatientAge,
    string? Relation,
    string Condition);

public sealed record LogMedicationDoseRequest(string? Notes);

public sealed record CreateMedicationRequest(
    string Name,
    string Dose,
    string Frequency,
    string? StartDate,
    string? EndDate);

public sealed record CreateAppointmentRequest(
    string Title,
    string Type,
    string ScheduledAt,
    string? Notes);

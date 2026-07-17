using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuroViva.Application.Common.Authorization;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.MedicalRecords;
using NeuroViva.Application.MedicalRecords.Commands.UploadClinicalNote;
using NeuroViva.Application.MedicalRecords.Commands.UploadExam;
using NeuroViva.Application.MedicalRecords.Queries.GetClinicalNotes;
using NeuroViva.Application.MedicalRecords.Queries.GetExams;
using NeuroViva.Application.MedicalRecords.Queries.GetFollowUp;

namespace NeuroViva.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/patients/{patientId:guid}")]
[Authorize(Policy = Policies.CaregiverOrDoctor)]
public sealed class MedicalRecordController : ControllerBase
{
    private readonly IMediator _mediator;

    public MedicalRecordController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Returns all exams for the specified patient.
    /// </summary>
    [HttpGet("exams")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExams(Guid patientId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamsQuery(patientId), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Validation => BadRequest(result.Error.Message),
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Uploads a new exam record with optional attachments (up to 5 files, 10 MB each).
    /// </summary>
    [HttpPost("exams")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(60 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadExam(
        Guid patientId,
        [FromForm] UploadExamRequest request,
        CancellationToken ct)
    {
        var attachments = new List<AttachmentInput>();
        if (request.Attachments is not null)
        {
            foreach (var file in request.Attachments)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, ct);
                attachments.Add(new AttachmentInput(ms.ToArray(), file.FileName, file.ContentType));
            }
        }

        var command = new UploadExamCommand(
            PatientId: patientId,
            Description: request.Description,
            EventDate: request.EventDate,
            Attachments: attachments);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Validation => BadRequest(result.Error.Message),
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new { recordId = result.Value.RecordId });
    }

    /// <summary>
    /// Returns all clinical notes (consultation, note, other) for the specified patient.
    /// </summary>
    [HttpGet("clinical-notes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClinicalNotes(Guid patientId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetClinicalNotesQuery(patientId), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Validation => BadRequest(result.Error.Message),
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Uploads a new clinical note with optional attachments (up to 5 files, 10 MB each).
    /// EventType must be one of: consultation, note, other.
    /// </summary>
    [HttpPost("clinical-notes")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(60 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadClinicalNote(
        Guid patientId,
        [FromForm] UploadClinicalNoteRequest request,
        CancellationToken ct)
    {
        var attachments = new List<AttachmentInput>();
        if (request.Attachments is not null)
        {
            foreach (var file in request.Attachments)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, ct);
                attachments.Add(new AttachmentInput(ms.ToArray(), file.FileName, file.ContentType));
            }
        }

        var command = new UploadClinicalNoteCommand(
            PatientId: patientId,
            EventType: request.EventType,
            Description: request.Description,
            EventDate: request.EventDate,
            Attachments: attachments);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Validation => BadRequest(result.Error.Message),
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new { recordId = result.Value.RecordId });
    }

    /// <summary>
    /// Returns the unified follow-up timeline (symptoms, appointments, medication logs) for the specified patient.
    /// </summary>
    [HttpGet("follow-up")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowUp(Guid patientId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFollowUpQuery(patientId), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Validation => BadRequest(result.Error.Message),
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }
}

public sealed class UploadExamRequest
{
    public string Description { get; set; } = default!;
    public DateTime? EventDate { get; set; }
    public IReadOnlyList<IFormFile>? Attachments { get; set; }
}

public sealed class UploadClinicalNoteRequest
{
    public string EventType { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime? EventDate { get; set; }
    public IReadOnlyList<IFormFile>? Attachments { get; set; }
}

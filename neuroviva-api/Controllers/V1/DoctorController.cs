using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuroViva.Application.Common.Authorization;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Doctors.Commands.MarkAlertSeen;
using NeuroViva.Application.Doctors.Commands.ResolveAlert;
using NeuroViva.Application.Doctors.Queries.GetDoctorAlerts;
using NeuroViva.Application.Doctors.Queries.GetDoctorPatients;
using NeuroViva.Application.Doctors.Queries.GetDoctors;
using NeuroViva.Application.Doctors.Queries.LookupDoctor;

namespace NeuroViva.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/doctor")]
public sealed class DoctorController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Returns all available doctors (for caregiver doctor selection).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Policies.Authenticated)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDoctors(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDoctorsQuery(), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns all active patients assigned to the authenticated doctor.
    /// </summary>
    [HttpGet("patients")]
    [Authorize(Policy = Policies.DoctorOnly)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatients(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDoctorPatientsQuery(), ct);

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
    /// Returns alerts for the authenticated doctor, optionally including resolved ones.
    /// </summary>
    [HttpGet("alerts")]
    [Authorize(Policy = Policies.DoctorOnly)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] bool includeResolved = false,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDoctorAlertsQuery(includeResolved), ct);

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
    /// Marks an alert as seen by the authenticated doctor.
    /// </summary>
    [HttpPatch("alerts/{id:guid}/seen")]
    [Authorize(Policy = Policies.DoctorOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAlertSeen(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new MarkAlertSeenCommand(id), ct);

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
    /// Resolves an alert belonging to the authenticated doctor.
    /// </summary>
    [HttpPatch("alerts/{id:guid}/resolve")]
    [Authorize(Policy = Policies.DoctorOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveAlert(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ResolveAlertCommand(id), ct);

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
    /// Looks up a doctor by medical license. Accessible by caregivers and doctors.
    /// </summary>
    [HttpGet("lookup")]
    [Authorize(Policy = Policies.Authenticated)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupDoctor(
        [FromQuery] string medicalLicense,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new LookupDoctorQuery(medicalLicense), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Validation => BadRequest(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }
}

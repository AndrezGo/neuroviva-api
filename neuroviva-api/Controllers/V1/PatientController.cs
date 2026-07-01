using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuroViva.Application.Common.Authorization;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Patients.Commands.ClaimPatientProfile;
using NeuroViva.Application.Patients.Queries.GetProfile;

namespace NeuroViva.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/patient")]
[Authorize(Policy = Policies.PatientOnly)]
public sealed class PatientController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Returns the patient profile linked to the authenticated user.
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPatientProfileQuery(), ct);

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
    /// Claims an existing patient profile (by document number) for the authenticated user,
    /// or creates a new patient profile if none exists with that document number.
    /// </summary>
    [HttpPost("claim")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ClaimProfile(
        [FromBody] ClaimPatientRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ClaimPatientProfileCommand(request.DocumentNumber), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Conflict => StatusCode(StatusCodes.Status409Conflict, result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new { patientId = result.Value.PatientId });
    }
}

public sealed record ClaimPatientRequest(string DocumentNumber);

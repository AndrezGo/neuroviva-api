using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuroViva.Application.Ai.Commands.SendChatMessage;
using NeuroViva.Application.Ai.Queries.GetChatHistory;
using NeuroViva.Application.Common.Authorization;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/patients/{patientId:guid}/ai-assistant")]
[Authorize(Policy = Policies.DoctorOnly)]
public sealed class AiAssistantController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiAssistantController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Returns the persisted AI chat history for the given patient (scoped to the calling doctor).
    /// </summary>
    [HttpGet("messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(Guid patientId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetChatHistoryQuery(patientId), ct);

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
    /// Sends a message to the AI assistant and returns the assistant's reply.
    /// The conversation is persisted and can be retrieved via GET messages.
    /// </summary>
    [HttpPost("messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessage(
        Guid patientId,
        [FromBody] SendChatMessageRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new SendChatMessageCommand(patientId, request.Message), ct);

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

public sealed class SendChatMessageRequest
{
    public string Message { get; set; } = default!;
}

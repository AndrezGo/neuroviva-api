using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuroViva.Application.Admin.Commands.BackfillPdfExtraction;
using NeuroViva.Application.Common.Authorization;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Policy = Policies.AdminOnly)]
public sealed class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// One-shot backfill: for every PDF attachment that has no extracted text,
    /// downloads the file from storage, extracts its text via PdfPig and persists it.
    /// Run once from Postman/curl after the extracted_text column migration is applied.
    /// Protected by AdminOnly policy.
    /// </summary>
    [HttpPost("backfill-pdf-extraction")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BackfillPdfExtraction(
        [FromQuery] int batchSize = 200,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new BackfillPdfExtractionCommand(batchSize), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new
        {
            total = result.Value.Total,
            processed = result.Value.Processed,
            failed = result.Value.Failed
        });
    }
}

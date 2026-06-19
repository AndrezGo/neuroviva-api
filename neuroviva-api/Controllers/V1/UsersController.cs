using System.Security.Claims;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Authorization;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Features.Users.Commands;
using NeuroViva.Application.Features.Users.Queries;

namespace NeuroViva.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>Returns the authenticated user's profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), ct);
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
    /// Syncs the Supabase auth user with NeuroViva's internal user table.
    /// Call this once after the first successful login to associate the auth identity
    /// with a tenant. When TenantId is omitted a personal tenant is auto-created.
    /// Required before accessing any protected resources.
    /// </summary>
    [HttpPost("sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Sync([FromBody] SyncUserRequest request, CancellationToken ct)
    {
        var sub = User.FindFirst(ClaimNames.Sub)?.Value
               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(sub, out var authUserId))
            return Unauthorized("Invalid token: missing sub claim.");

        var command = new SyncUserCommand(
            AuthUserId: authUserId,
            Email: User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value
                ?? request.Email
                ?? string.Empty,
            Name: request.Name,
            TenantId: request.TenantId);  // may be null — auto-creates personal tenant

        var result = await _mediator.Send(command, ct);
        if (result.IsFailure) return BadRequest(result.Error.Message);

        return Ok(result.Value);
    }

    /// <summary>Assigns a role to the current authenticated user.</summary>
    [HttpPost("me/role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        if (_currentUser.UserId is null)
            return Unauthorized("User not synced. Call /users/sync first.");

        var command = new AssignRoleCommand(_currentUser.UserId.Value, request.RoleName);
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }
}

public sealed record SyncUserRequest(Guid? TenantId, string? Email, string? Name);
public sealed record AssignRoleRequest(string RoleName);

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuroViva.Application.Common.Authorization;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Community.Commands.AddReaction;
using NeuroViva.Application.Community.Commands.CreateComment;
using NeuroViva.Application.Community.Commands.CreatePost;
using NeuroViva.Application.Community.Commands.RemoveReaction;
using NeuroViva.Application.Community.Queries.GetGroupFeed;
using NeuroViva.Application.Community.Queries.GetMyGroups;
using NeuroViva.Application.Community.Queries.GetPostComments;
using NeuroViva.Application.Patients.Commands.ClaimPatientProfile;
using NeuroViva.Application.Patients.Queries.GetPatientFeed;
using NeuroViva.Application.Patients.Queries.GetProfile;
using NeuroViva.Domain.Content.Enums;

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

    [HttpGet("resources")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResources(
        [FromQuery] string type,
        [FromQuery] string? lang = null,
        CancellationToken ct = default)
    {
        var mapped = MapResourceType(type);
        if (mapped is null) return BadRequest($"Unknown resource type '{type}'. Allowed: news, scientific_article, video.");

        var normalizedLang = string.IsNullOrWhiteSpace(lang)
            ? "es"
            : lang.Trim().ToLowerInvariant();

        if (normalizedLang != "es" && normalizedLang != "en")
            return BadRequest($"Unknown lang '{lang}'. Allowed: es, en.");

        var result = await _mediator.Send(new GetPatientFeedQuery(mapped.Value, normalizedLang), ct);
        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        return Ok(result.Value);
    }

    // ── Community ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the list of community groups the patient belongs to (auto-joining groups
    /// matching their disease profile if not already a member).
    /// </summary>
    [HttpGet("community/groups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyGroups(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyGroupsQuery(), ct);
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
    /// Returns the paginated post feed for a community group.
    /// </summary>
    [HttpGet("community/groups/{groupId:guid}/feed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGroupFeed(
        Guid groupId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetGroupFeedQuery(groupId, skip, take), ct);
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
    /// Creates a new post in a community group.
    /// </summary>
    [HttpPost("community/groups/{groupId:guid}/posts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePost(
        Guid groupId,
        [FromBody] CreatePostRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreatePostCommand(groupId, request.Content, request.Visibility), ct);
        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                ErrorType.Conflict => StatusCode(StatusCodes.Status409Conflict, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        return Ok(new { postId = result.Value.PostId });
    }

    /// <summary>
    /// Adds a comment to a community post.
    /// </summary>
    [HttpPost("community/posts/{postId:guid}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateComment(
        Guid postId,
        [FromBody] CreateCommentRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateCommentCommand(postId, request.Content), ct);
        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error.Message),
                ErrorType.Conflict => StatusCode(StatusCodes.Status409Conflict, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        return Ok(new { commentId = result.Value.CommentId });
    }

    /// <summary>
    /// Returns the paginated comments for a community post.
    /// </summary>
    [HttpGet("community/posts/{postId:guid}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostComments(
        Guid postId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPostCommentsQuery(postId, skip, take), ct);
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
    /// Adds a reaction to a community post.
    /// </summary>
    [HttpPost("community/posts/{postId:guid}/reactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddReaction(
        Guid postId,
        [FromBody] AddReactionRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AddReactionCommand(postId, request.Type), ct);
        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Validation => BadRequest(result.Error.Message),
                ErrorType.Conflict => StatusCode(StatusCodes.Status409Conflict, result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        return Ok(new { reactionId = result.Value.ReactionId });
    }

    /// <summary>
    /// Removes a reaction from a community post (idempotent).
    /// </summary>
    [HttpDelete("community/posts/{postId:guid}/reactions/{type}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveReaction(
        Guid postId,
        string type,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveReactionCommand(postId, type), ct);
        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        return NoContent();
    }

    private static ResourceType? MapResourceType(string? raw) =>
        raw?.Trim().ToLowerInvariant() switch
        {
            "news" or "noticia" or "noticias" => ResourceType.News,
            "scientific_article" or "scientificarticle" or "article" or "articulo" or "articulo_cientifico" => ResourceType.ScientificArticle,
            "video" or "canal" or "canales" => ResourceType.Video,
            _ => null
        };
}

public sealed record ClaimPatientRequest(string DocumentNumber);

public sealed record CreatePostRequest(string Content, string? Visibility);

public sealed record CreateCommentRequest(string Content);

public sealed record AddReactionRequest(string Type);

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuroViva.Application.Common.Authorization;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Community.Commands.CreateGroup;
using NeuroViva.Application.Community.Commands.ModerateComment;
using NeuroViva.Application.Community.Commands.ModeratePost;
using NeuroViva.Application.Curation.Commands.ApproveResource;
using NeuroViva.Application.Curation.Commands.CreateChannel;
using NeuroViva.Application.Curation.Commands.CreateResource;
using NeuroViva.Application.Curation.Commands.RejectResource;
using NeuroViva.Application.Curation.Commands.UpdateChannel;
using NeuroViva.Application.Curation.Commands.UpdateResource;
using NeuroViva.Application.Curation.Queries.GetActiveDiseases;
using NeuroViva.Application.Curation.Queries.GetAllResources;
using NeuroViva.Application.Curation.Queries.GetChannels;
using NeuroViva.Application.Curation.Queries.GetPendingResources;
using NeuroViva.Domain.Content.Enums;

namespace NeuroViva.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/curator")]
[Authorize(Policy = Policies.ScientificCommittee)]
public sealed class CuratorController : ControllerBase
{
    private readonly IMediator _mediator;

    public CuratorController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Creates a new resource (news, scientific article, or video) in pending state.
    /// </summary>
    [HttpPost("resources")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateResource(
        [FromBody] CreateResourceRequest request,
        CancellationToken ct)
    {
        var typeValue = MapResourceType(request.Type);
        if (typeValue is null)
            return BadRequest($"Unknown resource type '{request.Type}'. Allowed: news, scientific_article, video.");

        var command = new CreateResourceCommand(
            Title: request.Title,
            Type: typeValue.Value,
            Url: request.Url,
            Description: request.Description,
            DiseaseId: request.DiseaseId,
            ChannelId: request.ChannelId);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Validation => BadRequest(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new { resourceId = result.Value.ResourceId });
    }

    /// <summary>
    /// Updates an existing resource. Does not change approval status.
    /// </summary>
    [HttpPatch("resources/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateResource(
        Guid id,
        [FromBody] UpdateResourceRequest request,
        CancellationToken ct)
    {
        var typeValue = MapResourceType(request.Type);
        if (typeValue is null)
            return BadRequest($"Unknown resource type '{request.Type}'. Allowed: news, scientific_article, video.");

        var command = new UpdateResourceCommand(
            Id: id,
            Title: request.Title,
            Type: typeValue.Value,
            Url: request.Url,
            Description: request.Description,
            DiseaseId: request.DiseaseId,
            ChannelId: request.ChannelId);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Validation => BadRequest(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return NoContent();
    }

    /// <summary>
    /// Returns all resources awaiting approval.
    /// </summary>
    [HttpGet("resources/pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPendingResources(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPendingResourcesQuery(), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns ALL resources regardless of approval status. Used by admin to find and edit already-approved resources.
    /// </summary>
    [HttpGet("resources")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllResources(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllResourcesQuery(), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns all active diseases for catalog/selector use (create resource, create group).
    /// </summary>
    [HttpGet("diseases")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveDiseases(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActiveDiseasesQuery(), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new channel (named container for video resources).
    /// </summary>
    [HttpPost("channels")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateChannel(
        [FromBody] CreateChannelRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateChannelCommand(request.Name, request.Description, request.AvatarUrl), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Validation => BadRequest(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new { channelId = result.Value.ChannelId });
    }

    /// <summary>
    /// Updates an existing channel.
    /// </summary>
    [HttpPatch("channels/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateChannel(
        Guid id,
        [FromBody] UpdateChannelRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateChannelCommand(id, request.Name, request.Description, request.AvatarUrl), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Validation => BadRequest(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return NoContent();
    }

    /// <summary>
    /// Returns all channels.
    /// </summary>
    [HttpGet("channels")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetChannels(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetChannelsQuery(), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(result.Value);
    }

    /// <summary>
    /// Approves a pending resource.
    /// </summary>
    [HttpPost("resources/{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveResource(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveResourceCommand(id), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return NoContent();
    }

    /// <summary>
    /// Rejects a pending resource.
    /// </summary>
    [HttpPost("resources/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectResource(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new RejectResourceCommand(id), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return NoContent();
    }

    // ── Community ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new community group.
    /// </summary>
    [HttpPost("community/groups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateGroup(
        [FromBody] CreateGroupRequest request,
        CancellationToken ct)
    {
        var command = new CreateGroupCommand(
            Name: request.Name,
            Slug: request.Slug,
            Description: request.Description,
            AvatarUrl: request.AvatarUrl,
            Visibility: request.Visibility ?? "public",
            DiseaseId: request.DiseaseId);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Conflict => StatusCode(StatusCodes.Status409Conflict, result.Error.Message),
                ErrorType.Validation => BadRequest(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return Ok(new { groupId = result.Value.GroupId });
    }

    /// <summary>
    /// Moderates (removes) a community post.
    /// </summary>
    [HttpPost("community/posts/{postId:guid}/moderate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ModeratePost(
        Guid postId,
        [FromBody] ModerateRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ModeratePostCommand(postId, request.Reason), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Validation => BadRequest(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };

        return NoContent();
    }

    /// <summary>
    /// Moderates (removes) a community comment.
    /// </summary>
    [HttpPost("community/comments/{commentId:guid}/moderate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ModerateComment(
        Guid commentId,
        [FromBody] ModerateRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ModerateCommentCommand(commentId, request.Reason), ct);

        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error.Message),
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Validation => BadRequest(result.Error.Message),
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

public sealed record CreateResourceRequest(
    string Title,
    string Type,
    string? Url,
    string? Description,
    Guid? DiseaseId,
    Guid? ChannelId);

public sealed record CreateChannelRequest(string Name, string? Description, string? AvatarUrl);

public sealed record UpdateChannelRequest(string Name, string? Description, string? AvatarUrl);

public sealed record UpdateResourceRequest(
    string Title,
    string Type,
    string? Url,
    string? Description,
    Guid? DiseaseId,
    Guid? ChannelId);

public sealed record CreateGroupRequest(
    string Name,
    string Slug,
    string? Description,
    string? AvatarUrl,
    string? Visibility,
    Guid? DiseaseId);

public sealed record ModerateRequest(string Reason);

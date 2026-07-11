using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Patients.Queries.GetPatientFeed;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Content.Enums;
using NeuroViva.Domain.Content.Repositories;

namespace NeuroViva.Application.Curation.Commands.UpdateResource;

public sealed class UpdateResourceCommandHandler
    : IRequestHandler<UpdateResourceCommand, Result>
{
    private readonly IResourceRepository _resourceRepo;
    private readonly IUnitOfWork _uow;

    public UpdateResourceCommandHandler(
        IResourceRepository resourceRepo,
        IUnitOfWork uow)
    {
        _resourceRepo = resourceRepo;
        _uow = uow;
    }

    public async Task<Result> Handle(
        UpdateResourceCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await _resourceRepo.GetByIdAsync(request.Id, cancellationToken);
        if (resource is null)
            return Error.NotFound("resource.not_found", "Resource not found");

        if (string.IsNullOrWhiteSpace(request.Title))
            return Error.Validation("resource.title_required", "Title is required.");

        if (request.ChannelId is not null && request.Type != ResourceType.Video)
            return Error.Validation("resource.channel_requires_video", "Channel can only be assigned to video resources.");

        if (request.Type == ResourceType.Video)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                return Error.Validation(
                    "resource.video_url_required",
                    "A YouTube URL is required for video resources.");

            if (YouTubeUrlParser.TryGetEmbedUrl(request.Url) is null)
                return Error.Validation(
                    "resource.invalid_video_url",
                    "The provided URL is not a valid YouTube video URL. Supported formats: youtube.com/watch?v=..., youtu.be/..., youtube.com/embed/..., youtube.com/shorts/...");
        }

        resource.Update(request.Title.Trim(), request.Type, request.Url, request.Description, request.DiseaseId, request.ChannelId);
        _resourceRepo.Update(resource);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}

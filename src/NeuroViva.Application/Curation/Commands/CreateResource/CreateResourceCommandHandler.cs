using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Patients.Queries.GetPatientFeed;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Content;
using NeuroViva.Domain.Content.Enums;
using NeuroViva.Domain.Content.Repositories;

namespace NeuroViva.Application.Curation.Commands.CreateResource;

public sealed class CreateResourceCommandHandler
    : IRequestHandler<CreateResourceCommand, Result<CreateResourceResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IResourceRepository _resourceRepo;
    private readonly IUnitOfWork _uow;

    public CreateResourceCommandHandler(
        ICurrentUserService currentUser,
        IResourceRepository resourceRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _resourceRepo = resourceRepo;
        _uow = uow;
    }

    public async Task<Result<CreateResourceResult>> Handle(
        CreateResourceCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        if (string.IsNullOrWhiteSpace(request.Title))
            return Error.Validation("resource.title_required", "Title is required.");

        if (request.Type == ResourceType.Video)
        {
            if (string.IsNullOrWhiteSpace(request.Url) ||
                YouTubeUrlParser.TryGetEmbedUrl(request.Url) is null)
                return Error.Validation(
                    "resource.invalid_video_url",
                    "Only YouTube URLs are supported for now.");
        }

        var resource = Resource.Create(
            authorId: _currentUser.UserId.Value,
            title: request.Title.Trim(),
            type: request.Type,
            diseaseId: request.DiseaseId,
            url: request.Url,
            description: request.Description);

        await _resourceRepo.AddAsync(resource, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new CreateResourceResult(resource.Id);
    }
}

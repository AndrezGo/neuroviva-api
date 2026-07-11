using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Content.Repositories;

namespace NeuroViva.Application.Curation.Queries.GetAllResources;

public sealed class GetAllResourcesQueryHandler
    : IRequestHandler<GetAllResourcesQuery, Result<IReadOnlyList<ResourceListItemDto>>>
{
    private readonly IResourceRepository _resourceRepo;
    private readonly IChannelRepository _channelRepo;

    public GetAllResourcesQueryHandler(
        IResourceRepository resourceRepo,
        IChannelRepository channelRepo)
    {
        _resourceRepo = resourceRepo;
        _channelRepo = channelRepo;
    }

    public async Task<Result<IReadOnlyList<ResourceListItemDto>>> Handle(
        GetAllResourcesQuery request,
        CancellationToken cancellationToken)
    {
        var resources = await _resourceRepo.ListAllAsync(cancellationToken);

        var channelIds = resources
            .Where(r => r.ChannelId.HasValue)
            .Select(r => r.ChannelId!.Value)
            .Distinct()
            .ToList();

        var channelNameById = channelIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await _channelRepo.ListByIdsAsync(channelIds, cancellationToken))
                .ToDictionary(c => c.Id, c => c.Name);

        var dtos = resources.Select(r => new ResourceListItemDto(
            Id: r.Id,
            AuthorId: r.AuthorId,
            DiseaseId: r.DiseaseId,
            Title: r.Title,
            Type: r.Type.ToString(),
            Url: r.Url,
            Description: r.Description,
            CreatedAt: r.CreatedAt,
            ChannelId: r.ChannelId,
            ChannelName: r.ChannelId.HasValue && channelNameById.TryGetValue(r.ChannelId.Value, out var cname) ? cname : null,
            ApprovalStatus: r.ApprovalStatus
        )).ToList();

        return Result<IReadOnlyList<ResourceListItemDto>>.Success(dtos);
    }
}

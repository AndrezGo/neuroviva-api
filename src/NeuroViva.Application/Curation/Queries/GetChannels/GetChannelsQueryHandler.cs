using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Content.Repositories;

namespace NeuroViva.Application.Curation.Queries.GetChannels;

public sealed class GetChannelsQueryHandler
    : IRequestHandler<GetChannelsQuery, Result<IReadOnlyList<ChannelDto>>>
{
    private readonly IChannelRepository _channelRepo;

    public GetChannelsQueryHandler(IChannelRepository channelRepo)
        => _channelRepo = channelRepo;

    public async Task<Result<IReadOnlyList<ChannelDto>>> Handle(
        GetChannelsQuery request,
        CancellationToken cancellationToken)
    {
        var channels = await _channelRepo.ListAllAsync(cancellationToken);

        var dtos = channels.Select(c => new ChannelDto(
            Id: c.Id,
            Name: c.Name,
            Description: c.Description,
            AvatarUrl: c.AvatarUrl,
            CreatedAt: c.CreatedAt
        )).ToList();

        return Result<IReadOnlyList<ChannelDto>>.Success(dtos);
    }
}

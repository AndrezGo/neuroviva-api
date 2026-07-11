using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Curation.Queries.GetChannels;

public sealed record GetChannelsQuery : IRequest<Result<IReadOnlyList<ChannelDto>>>;

public sealed record ChannelDto(
    Guid Id,
    string Name,
    string? Description,
    string? AvatarUrl,
    DateTime CreatedAt);

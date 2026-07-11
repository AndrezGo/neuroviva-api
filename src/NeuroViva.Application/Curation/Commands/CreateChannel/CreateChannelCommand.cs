using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Curation.Commands.CreateChannel;

public sealed record CreateChannelCommand(
    string Name,
    string? Description,
    string? AvatarUrl) : IRequest<Result<CreateChannelResult>>;

public sealed record CreateChannelResult(Guid ChannelId);

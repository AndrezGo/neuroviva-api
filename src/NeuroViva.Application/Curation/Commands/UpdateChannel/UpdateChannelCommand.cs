using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Curation.Commands.UpdateChannel;

public sealed record UpdateChannelCommand(
    Guid Id,
    string Name,
    string? Description,
    string? AvatarUrl) : IRequest<Result>;

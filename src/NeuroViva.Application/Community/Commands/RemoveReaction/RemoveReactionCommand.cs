using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Community.Commands.RemoveReaction;

public sealed record RemoveReactionCommand(
    Guid PostId,
    string Type) : IRequest<Result<RemoveReactionResult>>;

public sealed record RemoveReactionResult;

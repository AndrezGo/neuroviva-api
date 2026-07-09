using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Community.Commands.AddReaction;

public sealed record AddReactionCommand(
    Guid PostId,
    string Type) : IRequest<Result<AddReactionResult>>;

public sealed record AddReactionResult(Guid ReactionId);

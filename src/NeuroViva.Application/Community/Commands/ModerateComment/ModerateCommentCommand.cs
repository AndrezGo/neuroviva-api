using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Community.Commands.ModerateComment;

public sealed record ModerateCommentCommand(
    Guid CommentId,
    string Reason) : IRequest<Result<ModerateCommentResult>>;

public sealed record ModerateCommentResult;

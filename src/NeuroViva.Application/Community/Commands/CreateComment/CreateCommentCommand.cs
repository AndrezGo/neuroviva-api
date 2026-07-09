using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Community.Commands.CreateComment;

public sealed record CreateCommentCommand(
    Guid PostId,
    string Content) : IRequest<Result<CreateCommentResult>>;

public sealed record CreateCommentResult(Guid CommentId);

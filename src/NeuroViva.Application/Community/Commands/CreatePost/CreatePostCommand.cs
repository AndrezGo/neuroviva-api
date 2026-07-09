using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Community.Commands.CreatePost;

public sealed record CreatePostCommand(
    Guid GroupId,
    string Content,
    string? Visibility) : IRequest<Result<CreatePostResult>>;

public sealed record CreatePostResult(Guid PostId);

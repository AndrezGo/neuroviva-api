using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Community.Commands.ModeratePost;

public sealed record ModeratePostCommand(
    Guid PostId,
    string Reason) : IRequest<Result<ModeratePostResult>>;

public sealed record ModeratePostResult;

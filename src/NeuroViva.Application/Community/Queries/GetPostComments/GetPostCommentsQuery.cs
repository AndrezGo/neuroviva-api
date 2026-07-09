using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Community.Queries.GetPostComments;

public sealed record GetPostCommentsQuery(
    Guid PostId,
    int Skip = 0,
    int Take = 20) : IRequest<Result<IReadOnlyList<CommentFeedItemDto>>>;

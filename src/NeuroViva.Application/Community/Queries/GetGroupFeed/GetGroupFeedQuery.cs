using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Community.Queries.GetGroupFeed;

public sealed record GetGroupFeedQuery(
    Guid GroupId,
    int Skip = 0,
    int Take = 20) : IRequest<Result<IReadOnlyList<PostFeedItemDto>>>;

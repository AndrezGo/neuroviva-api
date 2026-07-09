using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Community.Queries.GetMyGroups;

public sealed record GetMyGroupsQuery : IRequest<Result<IReadOnlyList<GroupSummaryDto>>>;

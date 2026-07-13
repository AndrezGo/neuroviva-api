using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Content.Enums;

namespace NeuroViva.Application.Patients.Queries.GetPatientFeed;

public sealed record GetPatientFeedQuery(ResourceType Type, string Language, Guid? ChannelId = null) : IRequest<Result<IReadOnlyList<ResourceDto>>>;

using MediatR;
using NeuroViva.Application.Caregivers.Queries.GetToday;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetToday;

public sealed record GetCaregiverTodayQuery : IRequest<Result<CaregiverTodayDto>>;

using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetClinicalHistory;

public sealed record GetClinicalHistoryQuery
    : IRequest<Result<IReadOnlyList<HistoryEventDto>>>;

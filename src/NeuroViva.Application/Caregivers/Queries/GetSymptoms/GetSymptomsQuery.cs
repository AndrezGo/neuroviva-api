using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetSymptoms;

public sealed record GetSymptomsQuery : IRequest<Result<IReadOnlyList<SymptomListItemDto>>>;

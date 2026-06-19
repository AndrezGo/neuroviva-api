using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetMedications;

public sealed record GetMedicationsQuery : IRequest<Result<IReadOnlyList<MedicationListItemDto>>>;

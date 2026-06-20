using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetMedicationLogs;

public sealed record GetMedicationLogsQuery(Guid MedicationId)
    : IRequest<Result<IReadOnlyList<MedicationLogItemDto>>>;

using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.LogMedicationDose;

public sealed record LogMedicationDoseCommand(
    Guid MedicationId,
    string? Notes,
    bool? Taken = null
) : IRequest<Result<LogMedicationDoseResult>>;

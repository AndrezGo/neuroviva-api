using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.CreateMedication;

public sealed record CreateMedicationCommand(
    string Name,
    string Dose,
    string Frequency,
    // Optional ISO date string (yyyy-MM-dd). Defaults to today when null.
    string? StartDate,
    // Optional ISO date string (yyyy-MM-dd).
    string? EndDate
) : IRequest<Result<CreateMedicationResult>>;

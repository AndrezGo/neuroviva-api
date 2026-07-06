using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.UpdateSymptom;

public sealed record UpdateSymptomCommand(
    Guid SymptomId,
    string Type,
    int Intensity,
    string? Description
) : IRequest<Result>;

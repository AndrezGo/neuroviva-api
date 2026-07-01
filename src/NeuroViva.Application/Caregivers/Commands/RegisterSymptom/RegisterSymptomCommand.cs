using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.RegisterSymptom;

public sealed record RegisterSymptomCommand(
    string Type,
    int Intensity,
    string? Description,
    DateTime? LoggedAt
) : IRequest<Result<RegisterSymptomResult>>;

using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.DiscontinueMedication;

public sealed record DiscontinueMedicationCommand(Guid MedicationId) : IRequest<Result>;

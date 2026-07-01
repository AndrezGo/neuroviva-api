using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.SubmitAppointmentOutcome;

public sealed record SubmitAppointmentOutcomeCommand(Guid AppointmentId, string Outcome) : IRequest<Result>;

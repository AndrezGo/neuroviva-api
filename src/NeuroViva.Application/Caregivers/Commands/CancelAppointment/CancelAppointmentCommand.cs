using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.CancelAppointment;

public sealed record CancelAppointmentCommand(Guid AppointmentId) : IRequest<Result>;

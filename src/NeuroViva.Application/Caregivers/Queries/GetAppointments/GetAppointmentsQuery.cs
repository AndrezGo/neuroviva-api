using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetAppointments;

public sealed record GetAppointmentsQuery : IRequest<Result<IReadOnlyList<AppointmentListItemDto>>>;

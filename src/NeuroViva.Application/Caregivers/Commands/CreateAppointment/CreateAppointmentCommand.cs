using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.CreateAppointment;

public sealed record CreateAppointmentCommand(
    string Title,
    // Free-text or enum-like type: "consulta"/"consultation", "examen"/"exam", etc.
    string Type,
    // ISO 8601 datetime string.
    string ScheduledAt,
    string? Notes
) : IRequest<Result<CreateAppointmentResult>>;

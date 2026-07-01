using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Doctors.Commands.MarkAlertSeen;

public sealed record MarkAlertSeenCommand(Guid AlertId) : IRequest<Result>;

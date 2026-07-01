using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Doctors.Commands.ResolveAlert;

public sealed record ResolveAlertCommand(Guid AlertId) : IRequest<Result>;

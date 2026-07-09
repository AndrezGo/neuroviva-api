using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Curation.Commands.RejectResource;

public sealed record RejectResourceCommand(Guid ResourceId) : IRequest<Result>;

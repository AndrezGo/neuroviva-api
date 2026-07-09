using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Curation.Commands.ApproveResource;

public sealed record ApproveResourceCommand(Guid ResourceId) : IRequest<Result>;

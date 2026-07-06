using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.DeleteSymptom;

public sealed record DeleteSymptomCommand(Guid SymptomId) : IRequest<Result>;

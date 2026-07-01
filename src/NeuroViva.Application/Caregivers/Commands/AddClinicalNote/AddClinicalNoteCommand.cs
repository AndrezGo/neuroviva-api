using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.AddClinicalNote;

public sealed record AddClinicalNoteCommand(
    string EventType,
    string Description,
    DateTime? EventDate) : IRequest<Result<AddClinicalNoteResult>>;

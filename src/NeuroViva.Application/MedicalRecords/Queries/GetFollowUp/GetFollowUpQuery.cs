using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.MedicalRecords.Queries.GetFollowUp;

public sealed record GetFollowUpQuery(Guid? PatientId)
    : IRequest<Result<IReadOnlyList<HistoryEventDto>>>;

using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.MedicalRecords.Queries.GetExams;

public sealed record GetExamsQuery(Guid? PatientId)
    : IRequest<Result<IReadOnlyList<ClinicalRecordDto>>>;

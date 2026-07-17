using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.MedicalRecords.Queries.GetClinicalNotes;

public sealed record GetClinicalNotesQuery(Guid? PatientId)
    : IRequest<Result<IReadOnlyList<ClinicalRecordDto>>>;

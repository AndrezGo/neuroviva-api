using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.LookupPatient;

public sealed record LookupPatientQuery(string DocumentNumber)
    : IRequest<Result<LookupPatientDto>>;

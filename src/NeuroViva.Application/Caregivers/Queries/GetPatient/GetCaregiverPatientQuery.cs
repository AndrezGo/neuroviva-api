using MediatR;
using NeuroViva.Application.Caregivers.Queries.GetPatient;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetPatient;

public sealed record GetCaregiverPatientQuery : IRequest<Result<CaregiverPatientDto>>;

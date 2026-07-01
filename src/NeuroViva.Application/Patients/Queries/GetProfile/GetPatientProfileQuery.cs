using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Patients.Queries.GetProfile;

public sealed record GetPatientProfileQuery : IRequest<Result<PatientProfileDto>>;

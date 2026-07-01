using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Queries.GetPatientDoctor;

public sealed record GetPatientDoctorQuery() : IRequest<Result<PatientDoctorDto?>>;

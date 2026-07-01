using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Doctors.Queries.GetDoctorPatients;

public sealed record GetDoctorPatientsQuery() : IRequest<Result<DoctorPatientDto[]>>;

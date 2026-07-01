using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Doctors.Queries.LookupDoctor;

public sealed record LookupDoctorQuery(string MedicalLicense) : IRequest<Result<LookupDoctorResult>>;

using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Doctors.Queries.GetDoctors;

public sealed record GetDoctorsQuery() : IRequest<Result<DoctorListItemDto[]>>;

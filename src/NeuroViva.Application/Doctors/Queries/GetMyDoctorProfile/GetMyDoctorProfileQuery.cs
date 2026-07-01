using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Doctors.Queries.GetMyDoctorProfile;

public sealed record GetMyDoctorProfileQuery : IRequest<Result<MyDoctorProfileDto?>>;

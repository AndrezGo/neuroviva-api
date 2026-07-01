using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Doctors.Queries.GetDoctorAlerts;

public sealed record GetDoctorAlertsQuery(bool IncludeResolved = false) : IRequest<Result<DoctorAlertDto[]>>;

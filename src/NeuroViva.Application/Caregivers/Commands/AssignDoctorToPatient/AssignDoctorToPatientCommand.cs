using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Caregivers.Commands.AssignDoctorToPatient;

public sealed record AssignDoctorToPatientCommand(Guid DoctorId) : IRequest<Result>;

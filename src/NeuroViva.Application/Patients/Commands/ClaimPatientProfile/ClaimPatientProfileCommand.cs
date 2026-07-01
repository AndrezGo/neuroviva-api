using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Patients.Commands.ClaimPatientProfile;

public sealed record ClaimPatientProfileCommand(string DocumentNumber)
    : IRequest<Result<ClaimPatientProfileResult>>;

using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Appointments.Enums;
using NeuroViva.Domain.Appointments.Repositories;
using NeuroViva.Domain.Exceptions;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Commands.SubmitAppointmentOutcome;

public sealed class SubmitAppointmentOutcomeCommandHandler
    : IRequestHandler<SubmitAppointmentOutcomeCommand, Result>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IUnitOfWork _uow;

    public SubmitAppointmentOutcomeCommandHandler(
        ICurrentUserService currentUser,
        ICaregiverRepository caregiverRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        IAppointmentRepository appointmentRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _caregiverRepo = caregiverRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _appointmentRepo = appointmentRepo;
        _uow = uow;
    }

    public async Task<Result> Handle(
        SubmitAppointmentOutcomeCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var caregiver = await _caregiverRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (caregiver is null)
            return Error.NotFound("caregiver.not_found", "Caregiver profile not found");

        var links = await _patientCaregiverRepo.GetActiveByCaregiverAsync(caregiver.Id, cancellationToken);
        var link = links.FirstOrDefault();
        if (link is null)
            return Error.NotFound("caregiver.no_patient", "Caregiver has no linked patient");

        var appointment = await _appointmentRepo.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment is null)
            return Error.NotFound("appointment.not_found", "Appointment not found");

        if (appointment.PatientId != link.Patient.Id)
            return Error.Forbidden("You are not authorized to submit an outcome for this appointment.");

        var outcome = request.Outcome.ToLowerInvariant();

        if (outcome == "attended")
        {
            var domainResult = appointment.MarkAsAttended();
            if (domainResult.IsFailure)
                return Error.Validation(domainResult.ErrorCode!, domainResult.ErrorMessage!);
        }
        else if (outcome == "missed")
        {
            var domainResult = appointment.MarkAsMissed(AppointmentMissReason.CaregiverConfirmed);
            if (domainResult.IsFailure)
                return Error.Validation(domainResult.ErrorCode!, domainResult.ErrorMessage!);
        }
        else if (outcome == "cancelled")
        {
            try
            {
                appointment.Cancel();
            }
            catch (BusinessRuleViolationException ex)
            {
                return Error.Conflict(ex.RuleCode, ex.Message);
            }
        }
        else
        {
            return Error.Validation("appointment.invalid_outcome",
                $"Outcome '{request.Outcome}' is not valid. Use 'attended', 'missed', or 'cancelled'.");
        }

        _appointmentRepo.Update(appointment);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}

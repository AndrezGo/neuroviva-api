using System.Globalization;
using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Appointments;
using NeuroViva.Domain.Appointments.Enums;
using NeuroViva.Domain.Appointments.Repositories;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Commands.CreateAppointment;

public sealed class CreateAppointmentCommandHandler
    : IRequestHandler<CreateAppointmentCommand, Result<CreateAppointmentResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IUnitOfWork _uow;

    public CreateAppointmentCommandHandler(
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

    public async Task<Result<CreateAppointmentResult>> Handle(
        CreateAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        // Resolve caregiver profile
        var caregiver = await _caregiverRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (caregiver is null)
            return Error.NotFound("caregiver.not_found", "Caregiver profile not found");

        // Resolve linked patient — take first active link (most recent by start_date)
        var links = await _patientCaregiverRepo.GetActiveByCaregiverAsync(caregiver.Id, cancellationToken);
        var link = links.FirstOrDefault();
        if (link is null)
            return Error.NotFound("caregiver.no_patient", "Caregiver has no linked patient");

        var scheduledAt = DateTime.Parse(
            request.ScheduledAt,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

        var type = MapType(request.Type);

        // The Appointment entity has no separate title column; the read side derives the
        // title from the first line of notes. Store the title as that first line.
        var notes = string.IsNullOrWhiteSpace(request.Notes)
            ? request.Title
            : $"{request.Title}\n{request.Notes}";

        var appointment = Appointment.Schedule(
            patientId: link.Patient.Id,
            type: type,
            scheduledAt: scheduledAt,
            notes: notes,
            doctorId: null);

        await _appointmentRepo.AddAsync(appointment, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new CreateAppointmentResult(appointment.Id);
    }

    /// <summary>
    /// Maps a free-text type (Spanish or English) to the AppointmentType enum.
    /// Defaults to Consultation when unrecognized.
    /// </summary>
    private static AppointmentType MapType(string raw) =>
        raw.Trim().ToLowerInvariant() switch
        {
            "exam" or "examen" => AppointmentType.Exam,
            "procedure" or "procedimiento" => AppointmentType.Procedure,
            "teleconsultation" or "teleconsulta" => AppointmentType.Teleconsultation,
            _ => AppointmentType.Consultation
        };
}

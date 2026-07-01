using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Commands.CreateInAppNotification;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.HealthMonitoring;
using NeuroViva.Domain.HealthMonitoring.Repositories;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Caregivers.Commands.RegisterSymptom;

public sealed class RegisterSymptomCommandHandler
    : IRequestHandler<RegisterSymptomCommand, Result<RegisterSymptomResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICaregiverRepository _caregiverRepo;
    private readonly IPatientCaregiverRepository _patientCaregiverRepo;
    private readonly ISymptomRepository _symptomRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMediator _mediator;

    public RegisterSymptomCommandHandler(
        ICurrentUserService currentUser,
        ICaregiverRepository caregiverRepo,
        IPatientCaregiverRepository patientCaregiverRepo,
        ISymptomRepository symptomRepo,
        IUnitOfWork uow,
        IMediator mediator)
    {
        _currentUser = currentUser;
        _caregiverRepo = caregiverRepo;
        _patientCaregiverRepo = patientCaregiverRepo;
        _symptomRepo = symptomRepo;
        _uow = uow;
        _mediator = mediator;
    }

    public async Task<Result<RegisterSymptomResult>> Handle(
        RegisterSymptomCommand request,
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

        var symptom = Symptom.Register(
            patientId: link.Patient.Id,
            loggedBy: _currentUser.UserId.Value,
            type: request.Type,
            intensity: request.Intensity,
            description: request.Description,
            loggedAt: request.LoggedAt);

        await _symptomRepo.AddAsync(symptom, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        try
        {
            await _mediator.Send(new CreateInAppNotificationCommand(
                _currentUser.UserId.Value,
                "Síntoma registrado",
                $"Se registró '{request.Type}' con severidad {request.Intensity}/10"), cancellationToken);
        }
        catch { }

        return new RegisterSymptomResult(symptom.Id);
    }
}

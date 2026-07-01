using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Exceptions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Users;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Doctors.Commands.CompleteOnboarding;

public sealed class CompleteDoctorOnboardingCommandHandler
    : IRequestHandler<CompleteDoctorOnboardingCommand, Result<CompleteDoctorOnboardingResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uow;

    public CompleteDoctorOnboardingCommandHandler(
        ICurrentUserService currentUser,
        IDoctorRepository doctorRepository,
        IUserRepository userRepository,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _doctorRepository = doctorRepository;
        _userRepository = userRepository;
        _uow = uow;
    }

    public async Task<Result<CompleteDoctorOnboardingResult>> Handle(
        CompleteDoctorOnboardingCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var userId = _currentUser.UserId!.Value;

        var existing = await _doctorRepository.GetByUserIdAsync(userId, cancellationToken);

        if (existing is not null)
        {
            var sameData = string.Equals(existing.Specialty, request.Specialty, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(existing.MedicalLicense, request.MedicalLicense, StringComparison.OrdinalIgnoreCase);

            if (sameData)
                return new CompleteDoctorOnboardingResult(
                    existing.Id,
                    existing.IsScientificCommittee,
                    AlreadyOnboarded: true);

            return Error.Conflict(
                "doctor.already_onboarded",
                "Ya tienes un perfil médico registrado.");
        }

        var licenseOwner = await _doctorRepository.GetByMedicalLicenseAsync(
            request.MedicalLicense, cancellationToken);

        if (licenseOwner is not null)
            return Error.Conflict(
                "doctor.license_taken",
                "La cédula profesional ya está registrada.");

        var doctor = Doctor.Create(userId, request.Specialty, request.MedicalLicense);

        try
        {
            await _doctorRepository.AddAsync(doctor, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is not null)
            {
                var fullName = $"{request.FirstName} {request.LastName}".Trim();
                user.UpdateName(fullName);
                _userRepository.Update(user);
                await _uow.SaveChangesAsync(cancellationToken);
            }
        }
        catch (UniqueConstraintViolationException)
        {
            // Race condition: another request inserted the same userId or medical license first.
            // Re-read and return the existing record so the caller gets idempotent behavior.
            var winner = await _doctorRepository.GetByUserIdAsync(userId, cancellationToken);
            if (winner is not null)
                return new CompleteDoctorOnboardingResult(
                    winner.Id,
                    winner.IsScientificCommittee,
                    AlreadyOnboarded: true);

            return Error.Conflict(
                "doctor.license_taken",
                "La cédula profesional ya está registrada.");
        }

        return new CompleteDoctorOnboardingResult(
            doctor.Id,
            doctor.IsScientificCommittee,
            AlreadyOnboarded: false);
    }
}

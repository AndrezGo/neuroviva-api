namespace NeuroViva.Application.Doctors.Commands.CompleteOnboarding;

public sealed record CompleteDoctorOnboardingResult(
    Guid DoctorId,
    bool IsScientificCommittee,
    bool AlreadyOnboarded
);

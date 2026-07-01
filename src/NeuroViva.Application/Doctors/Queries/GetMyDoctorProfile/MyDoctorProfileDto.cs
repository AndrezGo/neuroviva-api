namespace NeuroViva.Application.Doctors.Queries.GetMyDoctorProfile;

public sealed record MyDoctorProfileDto(
    Guid DoctorId,
    Guid UserId,
    string Specialty,
    string MedicalLicense,
    bool IsScientificCommittee,
    DateTime CreatedAt
);

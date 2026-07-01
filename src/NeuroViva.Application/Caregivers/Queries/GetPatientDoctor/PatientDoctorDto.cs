namespace NeuroViva.Application.Caregivers.Queries.GetPatientDoctor;

public sealed record PatientDoctorDto(Guid DoctorId, string Name, string? Specialty, string? MedicalLicense);

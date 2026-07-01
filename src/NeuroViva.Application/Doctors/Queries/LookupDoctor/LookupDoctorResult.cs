namespace NeuroViva.Application.Doctors.Queries.LookupDoctor;

public sealed record LookupDoctorResult(Guid DoctorId, string? Specialty, string? MedicalLicense);

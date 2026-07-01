namespace NeuroViva.Application.Doctors.Queries.GetDoctors;

public sealed record DoctorListItemDto(Guid DoctorId, string Name, string? Specialty, string? MedicalLicense);

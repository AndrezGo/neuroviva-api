using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Users.Repositories;

namespace NeuroViva.Application.Doctors.Queries.LookupDoctor;

public sealed class LookupDoctorQueryHandler
    : IRequestHandler<LookupDoctorQuery, Result<LookupDoctorResult>>
{
    private readonly IDoctorRepository _doctorRepo;

    public LookupDoctorQueryHandler(IDoctorRepository doctorRepo)
    {
        _doctorRepo = doctorRepo;
    }

    public async Task<Result<LookupDoctorResult>> Handle(
        LookupDoctorQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MedicalLicense))
            return Error.Validation("doctor.medical_license_required", "Medical license is required");

        var doctor = await _doctorRepo.GetByMedicalLicenseAsync(request.MedicalLicense.Trim(), cancellationToken);
        if (doctor is null)
            return Error.NotFound("doctor.not_found", "Doctor not found");

        return Result<LookupDoctorResult>.Success(
            new LookupDoctorResult(doctor.Id, doctor.Specialty, doctor.MedicalLicense));
    }
}

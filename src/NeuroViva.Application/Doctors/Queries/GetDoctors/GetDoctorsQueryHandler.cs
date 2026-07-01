using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Doctors.Queries.GetDoctors;

public sealed class GetDoctorsQueryHandler
    : IRequestHandler<GetDoctorsQuery, Result<DoctorListItemDto[]>>
{
    private readonly IDoctorReadRepository _doctorReadRepo;

    public GetDoctorsQueryHandler(IDoctorReadRepository doctorReadRepo)
    {
        _doctorReadRepo = doctorReadRepo;
    }

    public async Task<Result<DoctorListItemDto[]>> Handle(
        GetDoctorsQuery request,
        CancellationToken cancellationToken)
    {
        var list = await _doctorReadRepo.ListAllAsync(cancellationToken);
        return Result<DoctorListItemDto[]>.Success(list.ToArray());
    }
}

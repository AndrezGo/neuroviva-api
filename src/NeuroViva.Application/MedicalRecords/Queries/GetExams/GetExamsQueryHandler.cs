using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Common.Services;

namespace NeuroViva.Application.MedicalRecords.Queries.GetExams;

public sealed class GetExamsQueryHandler
    : IRequestHandler<GetExamsQuery, Result<IReadOnlyList<ClinicalRecordDto>>>
{
    private readonly IPatientAccessGuard _guard;
    private readonly IMedicalRecordReadRepository _readRepo;

    public GetExamsQueryHandler(
        IPatientAccessGuard guard,
        IMedicalRecordReadRepository readRepo)
    {
        _guard = guard;
        _readRepo = readRepo;
    }

    public async Task<Result<IReadOnlyList<ClinicalRecordDto>>> Handle(
        GetExamsQuery request,
        CancellationToken cancellationToken)
    {
        var guardResult = await _guard.ResolveAndAuthorizeAsync(request.PatientId, cancellationToken);
        if (guardResult.IsFailure)
            return guardResult.Error;

        var records = await _readRepo.ListExamsAsync(guardResult.Value, cancellationToken);
        return Result<IReadOnlyList<ClinicalRecordDto>>.Success(records);
    }
}

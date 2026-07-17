using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Common.Services;

namespace NeuroViva.Application.MedicalRecords.Queries.GetClinicalNotes;

public sealed class GetClinicalNotesQueryHandler
    : IRequestHandler<GetClinicalNotesQuery, Result<IReadOnlyList<ClinicalRecordDto>>>
{
    private readonly IPatientAccessGuard _guard;
    private readonly IMedicalRecordReadRepository _readRepo;

    public GetClinicalNotesQueryHandler(
        IPatientAccessGuard guard,
        IMedicalRecordReadRepository readRepo)
    {
        _guard = guard;
        _readRepo = readRepo;
    }

    public async Task<Result<IReadOnlyList<ClinicalRecordDto>>> Handle(
        GetClinicalNotesQuery request,
        CancellationToken cancellationToken)
    {
        var guardResult = await _guard.ResolveAndAuthorizeAsync(request.PatientId, cancellationToken);
        if (guardResult.IsFailure)
            return guardResult.Error;

        var records = await _readRepo.ListClinicalNotesAsync(guardResult.Value, cancellationToken);
        return Result<IReadOnlyList<ClinicalRecordDto>>.Success(records);
    }
}

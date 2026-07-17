using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Common.Services;

namespace NeuroViva.Application.MedicalRecords.Queries.GetFollowUp;

public sealed class GetFollowUpQueryHandler
    : IRequestHandler<GetFollowUpQuery, Result<IReadOnlyList<HistoryEventDto>>>
{
    private readonly IPatientAccessGuard _guard;
    private readonly IMedicalRecordReadRepository _readRepo;

    public GetFollowUpQueryHandler(
        IPatientAccessGuard guard,
        IMedicalRecordReadRepository readRepo)
    {
        _guard = guard;
        _readRepo = readRepo;
    }

    public async Task<Result<IReadOnlyList<HistoryEventDto>>> Handle(
        GetFollowUpQuery request,
        CancellationToken cancellationToken)
    {
        var guardResult = await _guard.ResolveAndAuthorizeAsync(request.PatientId, cancellationToken);
        if (guardResult.IsFailure)
            return guardResult.Error;

        var events = await _readRepo.ListFollowUpAsync(guardResult.Value, cancellationToken);
        return Result<IReadOnlyList<HistoryEventDto>>.Success(events);
    }
}

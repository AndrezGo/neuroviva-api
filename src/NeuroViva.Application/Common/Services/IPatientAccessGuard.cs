using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Common.Services;

public interface IPatientAccessGuard
{
    Task<Result<Guid>> ResolveAndAuthorizeAsync(Guid? requestedPatientId, CancellationToken ct);
}

using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Ai.Services;

public interface IPatientContextBuilder
{
    Task<Result<string>> BuildSystemPromptAsync(Guid patientId, CancellationToken ct);
}

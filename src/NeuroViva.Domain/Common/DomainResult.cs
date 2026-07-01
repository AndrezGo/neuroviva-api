namespace NeuroViva.Domain.Common;

/// <summary>
/// Lightweight result type for domain method outcomes, avoiding circular dependencies
/// between the Domain and Application layers.
/// </summary>
public sealed class DomainResult
{
    private DomainResult() { IsSuccess = true; }
    private DomainResult(string code, string message) { IsSuccess = false; ErrorCode = code; ErrorMessage = message; }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    public static readonly DomainResult Ok = new();
    public static DomainResult Failure(string code, string message) => new(code, message);
    public static DomainResult ValidationError(string code, string message) => new(code, message);
}

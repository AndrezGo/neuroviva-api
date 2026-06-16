namespace NeuroViva.Application.Common.Models;

public enum ErrorType { Validation, NotFound, Conflict, Unauthorized, Forbidden, Failure }

public readonly record struct Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Unauthorized(string message = "Unauthorized") => new("unauthorized", message, ErrorType.Unauthorized);
    public static Error Forbidden(string message = "Forbidden") => new("forbidden", message, ErrorType.Forbidden);
    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
}

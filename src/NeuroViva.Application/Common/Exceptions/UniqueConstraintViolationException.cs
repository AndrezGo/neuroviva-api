namespace NeuroViva.Application.Common.Exceptions;

/// <summary>
/// Thrown by Infrastructure when a database unique constraint is violated.
/// Allows Application handlers to catch constraint violations without
/// depending directly on EF Core or any specific database provider.
/// </summary>
public sealed class UniqueConstraintViolationException : Exception
{
    public string? ConstraintName { get; }

    public UniqueConstraintViolationException(string message, string? constraintName = null, Exception? inner = null)
        : base(message, inner)
    {
        ConstraintName = constraintName;
    }
}

using FluentValidation.Results;

namespace NeuroViva.Application.Common.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}

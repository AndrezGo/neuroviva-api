namespace NeuroViva.Application.Common.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
    DateOnly Today => DateOnly.FromDateTime(UtcNow);
}

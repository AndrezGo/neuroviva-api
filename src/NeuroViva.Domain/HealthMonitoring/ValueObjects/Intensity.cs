using NeuroViva.Domain.Common;
using NeuroViva.Domain.Exceptions;

namespace NeuroViva.Domain.HealthMonitoring.ValueObjects;

public sealed class Intensity : ValueObject
{
    public int Value { get; }

    private Intensity(int value) => Value = value;

    public static Intensity Of(int value)
    {
        if (value < 1 || value > 10)
            throw new BusinessRuleViolationException(
                "symptom.intensity_out_of_range",
                "Intensity must be between 1 and 10.");
        return new Intensity(value);
    }

    public bool IsHigh => Value >= 7;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

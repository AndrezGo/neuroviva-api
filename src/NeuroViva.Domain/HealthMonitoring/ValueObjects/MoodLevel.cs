using NeuroViva.Domain.Common;
using NeuroViva.Domain.Exceptions;

namespace NeuroViva.Domain.HealthMonitoring.ValueObjects;

public sealed class MoodLevel : ValueObject
{
    public int Value { get; }

    private MoodLevel(int value) => Value = value;

    public static MoodLevel Of(int value)
    {
        if (value < 1 || value > 5)
            throw new BusinessRuleViolationException(
                "mood.level_out_of_range",
                "Mood level must be between 1 and 5.");
        return new MoodLevel(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

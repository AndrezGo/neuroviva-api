using NeuroViva.Application.Common.Abstractions;

namespace NeuroViva.Infrastructure.ExternalServices.Clock;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

using NeuroViva.Domain.Common;

namespace NeuroViva.Domain.Abstractions;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}

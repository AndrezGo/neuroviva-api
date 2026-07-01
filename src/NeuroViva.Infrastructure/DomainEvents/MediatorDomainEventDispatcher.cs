using MediatR;
using NeuroViva.Application.Common.DomainEvents;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Common;

namespace NeuroViva.Infrastructure.DomainEvents;

public sealed class MediatorDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;

    public MediatorDomainEventDispatcher(IPublisher publisher) => _publisher = publisher;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(@event.GetType());
            var notification = Activator.CreateInstance(notificationType, @event)!;
            await _publisher.Publish(notification, cancellationToken);
        }
    }
}

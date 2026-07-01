using MediatR;
using NeuroViva.Domain.Common;

namespace NeuroViva.Application.Common.DomainEvents;

public sealed record DomainEventNotification<TEvent>(TEvent Event) : INotification
    where TEvent : IDomainEvent;

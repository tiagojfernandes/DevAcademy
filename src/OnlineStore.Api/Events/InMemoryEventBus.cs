using System.Collections.Concurrent;

namespace OnlineStore.Api.Events;

/// <summary>
/// In-process event bus.
///
/// Programming paradigm - Event-driven programming:
/// - Publishers do not know who will handle the event.
/// - Subscribers can be added without changing the publisher.
///
/// This implementation is intentionally simple. Real event buses need retries, observability,
/// duplicate handling (idempotency), etc.
/// </summary>
public sealed class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

    public void Publish<TEvent>(TEvent @event) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            return;
        }

        // Snapshot to avoid issues if handlers are added while publishing.
        foreach (var handler in handlers.ToArray())
        {
            ((Action<TEvent>)handler)(@event);
        }
    }

    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var list = _handlers.GetOrAdd(typeof(TEvent), _ => []);

        lock (list)
        {
            list.Add(handler);
        }
    }
}

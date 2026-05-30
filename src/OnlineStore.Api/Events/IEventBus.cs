namespace OnlineStore.Api.Events;

/// <summary>
/// Programming paradigm - Event-driven programming (publish/subscribe):
/// A producer publishes an event, and one or more consumers subscribe and react.
///
/// SOLID - Dependency Inversion Principle:
/// High-level code should depend on this abstraction rather than a concrete bus implementation.
/// </summary>
public interface IEventBus
{
    void Publish<TEvent>(TEvent @event) where TEvent : IEvent;

    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent;
}

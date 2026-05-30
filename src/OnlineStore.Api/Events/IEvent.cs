namespace OnlineStore.Api.Events;

/// <summary>
/// Programming paradigm - Event-driven programming:
/// An event is a fact that happened in the system.
///
/// In distributed systems, events may be published to a queue/topic acting as message broker.
/// In this project we use an in-process event bus to demonstrate the idea.
/// </summary>
public interface IEvent
{
    DateTimeOffset OccurredAt { get; }
}

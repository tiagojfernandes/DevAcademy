namespace OnlineStore.Api.Events;

public sealed record ProductCreatedEvent(int ProductId, string Name, DateTimeOffset OccurredAt) : IEvent;
public sealed record ProductUpdatedEvent(int ProductId, string Name, DateTimeOffset OccurredAt) : IEvent;
public sealed record ProductDeletedEvent(int ProductId, DateTimeOffset OccurredAt) : IEvent;

namespace OnlineStore.Api.Events;

public sealed record UserRegisteredEvent(int UserId, DateTimeOffset OccurredAt) : IEvent;

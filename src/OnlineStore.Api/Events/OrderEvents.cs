namespace OnlineStore.Api.Events;

public sealed record OrderPlacedEvent(int OrderId, int UserId, decimal TotalPrice, DateTimeOffset OccurredAt) : IEvent;

public sealed record OrderStatusChangedEvent(int OrderId, string OldStatus, string NewStatus, DateTimeOffset OccurredAt) : IEvent;

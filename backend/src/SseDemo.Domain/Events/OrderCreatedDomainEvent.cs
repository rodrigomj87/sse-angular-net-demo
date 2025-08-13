using SseDemo.Domain.Abstractions;

namespace SseDemo.Domain.Events;

public sealed class OrderCreatedDomainEvent(Guid orderId) : IDomainEvent
{
    public Guid OrderId { get; } = orderId;
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

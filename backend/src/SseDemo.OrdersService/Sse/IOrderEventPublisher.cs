using SseDemo.Domain.Entities;

namespace SseDemo.OrdersService.Sse;

public interface IOrderEventPublisher
{
    Task OrderCreatedAsync(Order order, CancellationToken ct = default);
    Task OrderStatusChangedAsync(Order order, string previousStatus, CancellationToken ct = default);
}

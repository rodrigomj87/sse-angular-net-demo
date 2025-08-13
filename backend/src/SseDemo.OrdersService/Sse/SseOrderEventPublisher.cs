using System.Text.Json;
using SseDemo.Domain.Entities;

namespace SseDemo.OrdersService.Sse;

internal sealed class SseOrderEventPublisher : IOrderEventPublisher
{
    private readonly ISseClientRegistry _registry;

    public SseOrderEventPublisher(ISseClientRegistry registry)
    {
        _registry = registry;
    }

    public Task OrderCreatedAsync(Order order, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            id = order.Id,
            code = order.Code,
            status = order.Status.ToString().ToLowerInvariant(),
            customerName = order.CustomerName,
            totalAmount = order.TotalAmount,
            createdAt = order.CreatedAt,
            updatedAt = order.UpdatedAt
        });
        return _registry.BroadcastAsync("order-created", payload, order.Id.ToString(), ct);
    }

    public Task OrderStatusChangedAsync(Order order, string previousStatus, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            id = order.Id,
            code = order.Code,
            previousStatus = previousStatus.ToLowerInvariant(),
            newStatus = order.Status.ToString().ToLowerInvariant(),
            updatedAt = order.UpdatedAt
        });
        return _registry.BroadcastAsync("order-status-changed", payload, order.Id.ToString(), ct);
    }
}

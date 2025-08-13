using SseDemo.Domain.Entities;

namespace SseDemo.OrdersService.Dtos;

public sealed record OrderResponse(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static OrderResponse From(Order order) => new(
        order.Id,
        order.CustomerName,
        order.TotalAmount,
        order.Status.ToString().ToUpperInvariant(),
        order.CreatedAt,
        order.UpdatedAt);
}

namespace SseDemo.OrdersService.Dtos;

public sealed class ListOrdersResponse
{
    public required IReadOnlyCollection<OrderResponse> Items { get; init; }
    public int Total { get; init; }
}

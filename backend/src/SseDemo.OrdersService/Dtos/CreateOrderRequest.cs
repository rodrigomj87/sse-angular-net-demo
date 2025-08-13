namespace SseDemo.OrdersService.Dtos;

public sealed class CreateOrderRequest
{
    public string? CustomerName { get; set; }
    public decimal? TotalAmount { get; set; }
}

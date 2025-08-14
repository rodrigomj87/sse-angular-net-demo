namespace SseDemo.OrdersService.Dtos;

public sealed record ErrorResponse(string Code, string Message, object? Errors = null);

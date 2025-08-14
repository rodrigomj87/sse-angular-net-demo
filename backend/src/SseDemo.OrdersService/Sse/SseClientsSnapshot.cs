namespace SseDemo.OrdersService.Sse;

public sealed record SseClientsSnapshot(int Count, DateTimeOffset CapturedAt);

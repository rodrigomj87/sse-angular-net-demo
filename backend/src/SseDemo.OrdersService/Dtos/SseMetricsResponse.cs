namespace SseDemo.OrdersService.Dtos;

/// <summary>
/// Snapshot de métricas SSE.
/// </summary>
public sealed record SseMetricsResponse(int ActiveConnections, DateTimeOffset CapturedAt);

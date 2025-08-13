namespace SseDemo.OrdersService.Sse;

/// <summary>
/// Registry of active SSE clients for broadcasting events in-memory.
/// </summary>
public interface ISseClientRegistry
{
    int ActiveCount { get; }
    Guid Register(HttpResponse response, CancellationToken requestAborted);
    bool Remove(Guid clientId);
    Task BroadcastAsync(string @event, string data, string? id = null, CancellationToken ct = default);
    Task BroadcastCommentAsync(string comment, CancellationToken ct = default);
}

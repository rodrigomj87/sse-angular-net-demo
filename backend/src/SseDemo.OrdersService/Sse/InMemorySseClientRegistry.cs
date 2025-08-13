using System.Collections.Concurrent;
using System.Text;

namespace SseDemo.OrdersService.Sse;

internal sealed class InMemorySseClientRegistry : ISseClientRegistry
{
    private readonly ConcurrentDictionary<Guid, HttpResponse> _clients = new();

    public int ActiveCount => _clients.Count;

    public Guid Register(HttpResponse response, CancellationToken requestAborted)
    {
        var id = Guid.NewGuid();
        _clients[id] = response;

        requestAborted.Register(() => Remove(id));
        return id;
    }

    public bool Remove(Guid clientId)
    {
        return _clients.TryRemove(clientId, out _);
    }

    public async Task BroadcastAsync(string @event, string data, string? id = null, CancellationToken ct = default)
    {
        if (_clients.IsEmpty) return;
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(@event)) sb.Append("event:").Append(@event).Append('\n');
        if (!string.IsNullOrEmpty(id)) sb.Append("id:").Append(id).Append('\n');
        foreach (var line in data.Split('\n'))
        {
            sb.Append("data:").Append(line).Append('\n');
        }
        sb.Append('\n');
        var payload = sb.ToString();
        var bytes = Encoding.UTF8.GetBytes(payload);

        var toRemove = new List<Guid>();

        foreach (var kvp in _clients)
        {
            var resp = kvp.Value;
            try
            {
                await resp.Body.WriteAsync(bytes, 0, bytes.Length, ct);
                await resp.Body.FlushAsync(ct);
            }
            catch
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var r in toRemove) _clients.TryRemove(r, out _);
    }
}

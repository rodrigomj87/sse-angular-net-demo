namespace SseDemo.OrdersService.Sse;

internal sealed class SseHeartbeatService : BackgroundService
{
    private readonly ISseClientRegistry _registry;
    private readonly ILogger<SseHeartbeatService> _logger;

    public SseHeartbeatService(ISseClientRegistry registry, ILogger<SseHeartbeatService> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _registry.BroadcastCommentAsync($"heartbeat {_registry.ActiveCount} connections", stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SSE heartbeat failed");
            }
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}

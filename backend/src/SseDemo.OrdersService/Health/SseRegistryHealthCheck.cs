using Microsoft.Extensions.Diagnostics.HealthChecks;
using SseDemo.OrdersService.Sse;

namespace SseDemo.OrdersService.Health;

public sealed class SseRegistryHealthCheck : IHealthCheck
{
    private readonly ISseClientRegistry _registry;
    public SseRegistryHealthCheck(ISseClientRegistry registry) => _registry = registry;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<string, object> data = new Dictionary<string, object>
        {
            ["activeClients"] = _registry.ActiveCount
        };
        return Task.FromResult(HealthCheckResult.Healthy("SSE registry available", data));
    }
}

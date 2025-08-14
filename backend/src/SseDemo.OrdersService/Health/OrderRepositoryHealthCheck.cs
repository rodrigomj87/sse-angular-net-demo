using Microsoft.Extensions.Diagnostics.HealthChecks;
using SseDemo.Domain.Abstractions;

namespace SseDemo.OrdersService.Health;

public sealed class OrderRepositoryHealthCheck : IHealthCheck
{
    private readonly IOrderRepository _repo;
    public OrderRepositoryHealthCheck(IOrderRepository repo) => _repo = repo;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _repo.ListAsync(0, 1, cancellationToken);
            IReadOnlyDictionary<string, object> data = new Dictionary<string, object>
            {
                ["sampleCount"] = list.Count
            };
            return HealthCheckResult.Healthy("Order repository reachable", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Order repository failure", ex);
        }
    }
}

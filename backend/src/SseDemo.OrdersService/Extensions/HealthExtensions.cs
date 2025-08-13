using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SseDemo.OrdersService.Extensions;

public static class HealthExtensions
{
    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<SseDemo.OrdersService.Health.SseRegistryHealthCheck>("sse_registry")
            .AddCheck<SseDemo.OrdersService.Health.OrderRepositoryHealthCheck>("orders_repository");
        return services;
    }

    public static IEndpointRouteBuilder MapAppHealth(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health");
        endpoints.MapHealthChecks("/ready", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (ctx, report) =>
            {
                ctx.Response.ContentType = "application/json";
                var payload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString().ToLowerInvariant(),
                    checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString().ToLowerInvariant(), data = e.Value.Data })
                });
                await ctx.Response.WriteAsync(payload);
            }
        });
        return endpoints;
    }
}

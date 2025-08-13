using SseDemo.Domain.Abstractions;
using SseDemo.OrdersService.Repositories;
using SseDemo.OrdersService.Sse;
using SseDemo.OrdersService.Tracing;

namespace SseDemo.OrdersService.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register application core services (controllers, swagger, health, repositories, SSE infra, tracing).
    /// </summary>
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddAppSwagger();
        services.AddAppHealthChecks();

        // Domain / data
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

        // SSE infrastructure
        services.AddSingleton<ISseClientRegistry, InMemorySseClientRegistry>();
        services.AddSingleton<IOrderEventPublisher, SseOrderEventPublisher>();
        services.AddHostedService<SseHeartbeatService>();

        // Tracing accessor
        services.AddSingleton<ITraceContextAccessor, TraceContextAccessor>();

        return services;
    }
}

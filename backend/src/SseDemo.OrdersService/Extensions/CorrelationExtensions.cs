using Serilog.Context;
using System.Diagnostics;
using SseDemo.OrdersService.Tracing;

namespace SseDemo.OrdersService.Extensions;

public static class CorrelationExtensions
{
    private const string TraceHeader = "x-trace-id";

    public static IApplicationBuilder UseTraceCorrelation(this IApplicationBuilder app)
    {
        app.Use(async (ctx, next) =>
        {
            if (!ctx.Request.Headers.TryGetValue(TraceHeader, out var traceId) || string.IsNullOrWhiteSpace(traceId))
            {
                traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("n");
                ctx.Request.Headers[TraceHeader] = traceId;
            }
            ctx.Response.Headers[TraceHeader] = traceId!;
            var accessor = ctx.RequestServices.GetRequiredService<ITraceContextAccessor>();
            accessor.TraceId = traceId!;
            using (LogContext.PushProperty("TraceId", traceId!))
            {
                await next();
            }
        });
        return app;
    }
}

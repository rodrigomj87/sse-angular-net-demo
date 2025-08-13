namespace SseDemo.OrdersService.Extensions;

/// <summary>
/// SSE-specific pipeline adjustments (headers, buffering) isolated from Program.cs
/// </summary>
public static class SsePipelineExtensions
{
    public static IApplicationBuilder UseSseHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (ctx, next) =>
        {
            if (ctx.Request.Path.StartsWithSegments("/sse"))
            {
                ctx.Response.Headers["Cache-Control"] = "no-cache";
                ctx.Response.Headers["X-Accel-Buffering"] = "no"; // disable buffering in reverse proxies like nginx
            }
            await next();
        });
    }
}

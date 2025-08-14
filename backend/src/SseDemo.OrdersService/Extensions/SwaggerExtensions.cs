using Microsoft.OpenApi.Models;

namespace SseDemo.OrdersService.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddAppSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Orders Service API",
                Version = "v1",
                Description = "API de pedidos com SSE broadcast de eventos (order-created, order-status-changed)"
            });
            o.OperationFilter<SseDemo.OrdersService.OpenApi.TraceIdHeaderOperationFilter>();
            o.SchemaFilter<SseDemo.OrdersService.OpenApi.OrderResponseExample>();
            o.SchemaFilter<SseDemo.OrdersService.OpenApi.ErrorResponseExample>();
        });
        return services;
    }

    public static IApplicationBuilder UseAppSwagger(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = string.Empty;
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders Service API v1");
                c.DocumentTitle = "Orders Service API";
            });
        }
        return app;
    }
}

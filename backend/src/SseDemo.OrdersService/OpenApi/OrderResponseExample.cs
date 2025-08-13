using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using SseDemo.OrdersService.Dtos;

namespace SseDemo.OrdersService.OpenApi;

internal sealed class OrderResponseExample : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(OrderResponse)) return;
        schema.Example = new OpenApiObject
        {
            ["id"] = new OpenApiString(Guid.NewGuid().ToString()),
            ["customerName"] = new OpenApiString("Alice"),
            ["totalAmount"] = new OpenApiDouble(123.45),
            ["status"] = new OpenApiString("created"),
            ["createdAt"] = new OpenApiString(DateTimeOffset.UtcNow.ToString("O")),
            ["updatedAt"] = new OpenApiString(DateTimeOffset.UtcNow.ToString("O"))
        };
    }
}

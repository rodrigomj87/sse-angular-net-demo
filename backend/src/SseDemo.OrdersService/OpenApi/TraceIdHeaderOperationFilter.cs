using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SseDemo.OrdersService.OpenApi;

/// <summary>
/// Adds optional x-trace-id header to operations for correlation visibility.
/// </summary>
internal sealed class TraceIdHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        if (operation.Parameters.Any(p => p.Name == "x-trace-id")) return;
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "x-trace-id",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Correlation trace id (if omitted server will generate)",
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}

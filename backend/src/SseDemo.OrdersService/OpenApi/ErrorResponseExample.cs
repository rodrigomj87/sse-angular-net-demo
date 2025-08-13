using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SseDemo.OrdersService.OpenApi;

/// <summary>
/// Adds an example for ErrorResponse schema.
/// </summary>
public class ErrorResponseExample : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.FullName == typeof(SseDemo.OrdersService.Dtos.ErrorResponse).FullName)
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["code"] = new Microsoft.OpenApi.Any.OpenApiString("validation_error"),
                ["message"] = new Microsoft.OpenApi.Any.OpenApiString("Validation failed"),
                ["errors"] = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["customerName"] = new Microsoft.OpenApi.Any.OpenApiString("Required"),
                    ["totalAmount"] = new Microsoft.OpenApi.Any.OpenApiString("Must be >= 0")
                }
            };
        }
    }
}

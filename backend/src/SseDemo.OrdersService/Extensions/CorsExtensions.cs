using Microsoft.AspNetCore.Cors.Infrastructure;

namespace SseDemo.OrdersService.Extensions;

/// <summary>
/// CORS registration (isolated from Program.cs)
/// </summary>
public static class CorsExtensions
{
    public const string AppCorsPolicy = "AppCors";

    public static IServiceCollection AddAppCors(this IServiceCollection services)
    {
        services.AddCors(o => o.AddPolicy(AppCorsPolicy, b =>
            b.WithOrigins(
                    "http://localhost:4200", // Angular dev server
                    "http://localhost:3000"   // Nginx container
                )
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials()
        ));
        return services;
    }

    public static IApplicationBuilder UseAppCors(this IApplicationBuilder app)
        => app.UseCors(AppCorsPolicy);
}

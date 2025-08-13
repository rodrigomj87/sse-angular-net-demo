using Serilog;

namespace SseDemo.OrdersService.Extensions;

public static class LoggingExtensions
{
    /// <summary>
    /// Configure Serilog for the host using configuration (appsettings etc.).
    /// </summary>
    public static IHostBuilder UseSerilogLogging(this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        hostBuilder.UseSerilog((ctx, cfg) => cfg
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console());
        return hostBuilder;
    }
}

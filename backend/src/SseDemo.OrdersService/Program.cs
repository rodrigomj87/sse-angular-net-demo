using Serilog;
using SseDemo.OrdersService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Logging configuration
builder.Host.UseSerilogLogging(builder.Configuration);

// Application services registration
builder.Services.AddAppServices();
builder.Services.AddAppCors();

var app = builder.Build();

// Middleware pipeline
app.UseAppSwagger(app.Environment);

app.UseSerilogRequestLogging();
app.UseTraceCorrelation();
app.UseAppCors();
app.UseSseHeaders();
app.MapAppHealth();

app.MapControllers();

app.Run();

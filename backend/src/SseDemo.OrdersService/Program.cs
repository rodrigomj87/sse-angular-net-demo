using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog basic configuration (will evolve for enrichment later)
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Repositories
builder.Services.AddSingleton<SseDemo.Domain.Abstractions.IOrderRepository, SseDemo.OrdersService.Repositories.InMemoryOrderRepository>();

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready"); // placeholder readiness

app.MapGet("/ping", () => Results.Ok(new { ok = true, service = "orders", ts = DateTimeOffset.UtcNow }));

app.Run();

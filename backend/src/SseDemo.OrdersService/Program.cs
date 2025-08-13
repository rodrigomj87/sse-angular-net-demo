using Serilog;
using SseDemo.Domain.Abstractions;
using SseDemo.Domain.Entities;
using SseDemo.OrdersService.Dtos;
using SseDemo.OrdersService.Sse;

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
// SSE registry
builder.Services.AddSingleton<ISseClientRegistry, InMemorySseClientRegistry>();
builder.Services.AddSingleton<IOrderEventPublisher, SseOrderEventPublisher>();
builder.Services.AddHostedService<SseHeartbeatService>();

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

var orders = app.MapGroup("/api/orders").WithTags("Orders");
// SSE streaming endpoint
app.MapGet("/sse/stream", async (HttpContext ctx, ISseClientRegistry registry) =>
{
    ctx.Response.Headers.Add("Content-Type", "text/event-stream");
    ctx.Response.Headers.Add("Cache-Control", "no-cache");
    ctx.Response.Headers.Add("Connection", "keep-alive");
    var id = registry.Register(ctx.Response, ctx.RequestAborted);
    // Initial comment to keep connection open
    await ctx.Response.WriteAsync(": connected \n\n");
    await ctx.Response.Body.FlushAsync();
    // Hold until client disconnect
    try { await Task.Delay(Timeout.Infinite, ctx.RequestAborted); }
    catch (TaskCanceledException) { }
    finally { registry.Remove(id); }
}).WithOpenApi(op => { op.Summary = "Fluxo SSE de eventos"; return op; });

orders.MapPost("/", async (CreateOrderRequest req, IOrderRepository repo, IOrderEventPublisher publisher, HttpContext http, CancellationToken ct) =>
{
    // Basic validation (simplified; could use FluentValidation later)
    var errors = new Dictionary<string, string>();
    if (string.IsNullOrWhiteSpace(req.CustomerName)) errors["customerName"] = "Required";
    if (req.TotalAmount is null || req.TotalAmount < 0) errors["totalAmount"] = "Must be >= 0";
    if (errors.Count > 0)
        return Results.BadRequest(new ErrorResponse("validation_error", "Validation failed", errors));

    var order = Order.Create(req.CustomerName!.Trim(), req.TotalAmount!.Value);
    await repo.AddAsync(order);
    // publish SSE event (fire & forget pattern acceptable here, but await to surface errors early)
    await publisher.OrderCreatedAsync(order, ct);
    var response = OrderResponse.From(order);
    var location = $"/api/orders/{order.Id}";
    return Results.Created(location, response);
})
.WithOpenApi(op => { op.Summary = "Cria um novo pedido"; return op; });

orders.MapGet("/", async (IOrderRepository repo) =>
{
    var list = await repo.ListAsync(0, 200);
    var items = list.Select(OrderResponse.From).ToArray();
    return Results.Ok(new ListOrdersResponse { Items = items, Total = items.Length });
})
.WithOpenApi(op => { op.Summary = "Lista pedidos"; return op; });

orders.MapGet("/{id:guid}", async (Guid id, IOrderRepository repo) =>
{
    var order = await repo.GetAsync(id);
    if (order == null) return Results.NotFound(new ErrorResponse("not_found", "Order not found"));
    return Results.Ok(OrderResponse.From(order));
})
.WithOpenApi(op => { op.Summary = "Obtém pedido por ID"; return op; });

app.Run();

using Serilog;
using Serilog.Context;
using SseDemo.Domain.Abstractions;
using SseDemo.Domain.Entities;
using SseDemo.OrdersService.Dtos;
using SseDemo.OrdersService.Sse;
using System.Diagnostics;
using SseDemo.OrdersService.Tracing;

var builder = WebApplication.CreateBuilder(args);

// Serilog basic configuration (will evolve for enrichment later)
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck<SseDemo.OrdersService.Health.SseRegistryHealthCheck>("sse_registry")
    .AddCheck<SseDemo.OrdersService.Health.OrderRepositoryHealthCheck>("orders_repository");

// Repositories
builder.Services.AddSingleton<SseDemo.Domain.Abstractions.IOrderRepository, SseDemo.OrdersService.Repositories.InMemoryOrderRepository>();
// SSE registry
builder.Services.AddSingleton<ISseClientRegistry, InMemorySseClientRegistry>();
builder.Services.AddSingleton<IOrderEventPublisher, SseOrderEventPublisher>();
builder.Services.AddHostedService<SseHeartbeatService>();
builder.Services.AddSingleton<ITraceContextAccessor, TraceContextAccessor>();

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
// Correlation / TraceId middleware
app.Use(async (ctx, next) =>
{
    const string TraceHeader = "x-trace-id";
    if (!ctx.Request.Headers.TryGetValue(TraceHeader, out var traceId) || string.IsNullOrWhiteSpace(traceId))
    {
        traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("n");
        ctx.Request.Headers[TraceHeader] = traceId;
    }
    ctx.Response.Headers[TraceHeader] = traceId!;
    var accessor = ctx.RequestServices.GetRequiredService<ITraceContextAccessor>();
    accessor.TraceId = traceId!;
    using (LogContext.PushProperty("TraceId", traceId!))
    {
        await next();
    }
});
app.MapHealthChecks("/health"); // liveness (process up)
app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString().ToLowerInvariant(),
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString().ToLowerInvariant(), data = e.Value.Data })
        });
        await ctx.Response.WriteAsync(payload);
    }
});

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

// SSE clients snapshot (diagnóstico simples)
app.MapGet("/sse/clients", (ISseClientRegistry registry) => Results.Ok(registry.GetSnapshot()))
    .WithOpenApi(op => { op.Summary = "Snapshot conexões SSE"; return op; });

orders.MapPost("/", async (CreateOrderRequest req, IOrderRepository repo, IOrderEventPublisher publisher, HttpContext http, CancellationToken ct, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("Orders.Create");
    // Basic validation (simplified; could use FluentValidation later)
    var errors = new Dictionary<string, string>();
    if (string.IsNullOrWhiteSpace(req.CustomerName)) errors["customerName"] = "Required";
    if (req.TotalAmount is null || req.TotalAmount < 0) errors["totalAmount"] = "Must be >= 0";
    if (errors.Count > 0)
        return Results.BadRequest(new ErrorResponse("validation_error", "Validation failed", errors));

    var order = Order.Create(req.CustomerName!.Trim(), req.TotalAmount!.Value);
    await repo.AddAsync(order);
    logger.LogInformation("Order created {OrderId} code={Code} amount={Amount}", order.Id, order.Code, order.TotalAmount);
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

orders.MapPost("/{id:guid}/pay", async (Guid id, IOrderRepository repo, IOrderEventPublisher publisher, CancellationToken ct, ILoggerFactory lf) =>
{
    var logger = lf.CreateLogger("Orders.Pay");
    var order = await repo.GetAsync(id);
    if (order == null) return Results.NotFound(new ErrorResponse("not_found", "Order not found"));
    var prev = order.Status.ToString();
    if (!order.MarkPaid()) return Results.BadRequest(new ErrorResponse("invalid_state", "Cannot mark as paid from current state"));
    await repo.UpdateAsync(order);
    logger.LogInformation("Order paid {OrderId} prev={Prev} new={New}", order.Id, prev, order.Status);
    await publisher.OrderStatusChangedAsync(order, prev, ct);
    return Results.Ok(OrderResponse.From(order));
}).WithOpenApi(op => { op.Summary = "Marca pedido como pago"; return op; });

orders.MapPost("/{id:guid}/fulfill", async (Guid id, IOrderRepository repo, IOrderEventPublisher publisher, CancellationToken ct, ILoggerFactory lf) =>
{
    var logger = lf.CreateLogger("Orders.Fulfill");
    var order = await repo.GetAsync(id);
    if (order == null) return Results.NotFound(new ErrorResponse("not_found", "Order not found"));
    var prev = order.Status.ToString();
    if (!order.MarkFulfilled()) return Results.BadRequest(new ErrorResponse("invalid_state", "Cannot fulfill from current state"));
    await repo.UpdateAsync(order);
    logger.LogInformation("Order fulfilled {OrderId} prev={Prev} new={New}", order.Id, prev, order.Status);
    await publisher.OrderStatusChangedAsync(order, prev, ct);
    return Results.Ok(OrderResponse.From(order));
}).WithOpenApi(op => { op.Summary = "Marca pedido como finalizado"; return op; });

orders.MapPost("/{id:guid}/cancel", async (Guid id, IOrderRepository repo, IOrderEventPublisher publisher, CancellationToken ct, ILoggerFactory lf) =>
{
    var logger = lf.CreateLogger("Orders.Cancel");
    var order = await repo.GetAsync(id);
    if (order == null) return Results.NotFound(new ErrorResponse("not_found", "Order not found"));
    var prev = order.Status.ToString();
    if (!order.Cancel()) return Results.BadRequest(new ErrorResponse("invalid_state", "Cannot cancel from current state"));
    await repo.UpdateAsync(order);
    logger.LogInformation("Order canceled {OrderId} prev={Prev} new={New}", order.Id, prev, order.Status);
    await publisher.OrderStatusChangedAsync(order, prev, ct);
    return Results.Ok(OrderResponse.From(order));
}).WithOpenApi(op => { op.Summary = "Cancela pedido"; return op; });

app.Run();

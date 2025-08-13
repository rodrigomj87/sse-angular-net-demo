using Microsoft.AspNetCore.Mvc;
using SseDemo.Domain.Abstractions;
using SseDemo.Domain.Entities;
using SseDemo.OrdersService.Dtos;
using SseDemo.OrdersService.Sse;

namespace SseDemo.OrdersService.Controllers;

/// <summary>
/// Order operations (create, list, get, status transitions)
/// </summary>
[ApiController]
[Route("api/[controller]")] // api/orders
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _repo;
    private readonly IOrderEventPublisher _publisher;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderRepository repo, IOrderEventPublisher publisher, ILogger<OrdersController> logger)
    {
        _repo = repo;
        _publisher = publisher;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        var errors = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(req.CustomerName)) errors["customerName"] = "Required";
        if (req.TotalAmount is null || req.TotalAmount < 0) errors["totalAmount"] = "Must be >= 0";
        if (errors.Count > 0) return BadRequest(new ErrorResponse("validation_error", "Validation failed", errors));

        var order = Order.Create(req.CustomerName!.Trim(), req.TotalAmount!.Value);
        await _repo.AddAsync(order, ct);
        _logger.LogInformation("Order created {OrderId} code={Code} amount={Amount}", order.Id, order.Code, order.TotalAmount);
        await _publisher.OrderCreatedAsync(order, ct);
        var response = OrderResponse.From(order);
        return Created($"/api/orders/{order.Id}", response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ListOrdersResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var list = await _repo.ListAsync(0, 200, ct);
        var items = list.Select(OrderResponse.From).ToArray();
        return Ok(new ListOrdersResponse { Items = items, Total = items.Length });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var order = await _repo.GetAsync(id, ct);
        if (order == null) return NotFound(new ErrorResponse("not_found", "Order not found"));
        return Ok(OrderResponse.From(order));
    }

    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Pay(Guid id, CancellationToken ct)
    {
        var order = await _repo.GetAsync(id, ct);
        if (order == null) return NotFound(new ErrorResponse("not_found", "Order not found"));
        var prev = order.Status.ToString();
        if (!order.MarkPaid()) return BadRequest(new ErrorResponse("invalid_state", "Cannot mark as paid from current state"));
        await _repo.UpdateAsync(order, ct);
        _logger.LogInformation("Order paid {OrderId} prev={Prev} new={New}", order.Id, prev, order.Status);
        await _publisher.OrderStatusChangedAsync(order, prev, ct);
        return Ok(OrderResponse.From(order));
    }

    [HttpPost("{id:guid}/fulfill")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Fulfill(Guid id, CancellationToken ct)
    {
        var order = await _repo.GetAsync(id, ct);
        if (order == null) return NotFound(new ErrorResponse("not_found", "Order not found"));
        var prev = order.Status.ToString();
        if (!order.MarkFulfilled()) return BadRequest(new ErrorResponse("invalid_state", "Cannot fulfill from current state"));
        await _repo.UpdateAsync(order, ct);
        _logger.LogInformation("Order fulfilled {OrderId} prev={Prev} new={New}", order.Id, prev, order.Status);
        await _publisher.OrderStatusChangedAsync(order, prev, ct);
        return Ok(OrderResponse.From(order));
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var order = await _repo.GetAsync(id, ct);
        if (order == null) return NotFound(new ErrorResponse("not_found", "Order not found"));
        var prev = order.Status.ToString();
        if (!order.Cancel()) return BadRequest(new ErrorResponse("invalid_state", "Cannot cancel from current state"));
        await _repo.UpdateAsync(order, ct);
        _logger.LogInformation("Order canceled {OrderId} prev={Prev} new={New}", order.Id, prev, order.Status);
        await _publisher.OrderStatusChangedAsync(order, prev, ct);
        return Ok(OrderResponse.From(order));
    }
}

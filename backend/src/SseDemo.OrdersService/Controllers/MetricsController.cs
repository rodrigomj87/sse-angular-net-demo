using Microsoft.AspNetCore.Mvc;
using SseDemo.OrdersService.Dtos;
using SseDemo.OrdersService.Sse;

namespace SseDemo.OrdersService.Controllers;

/// <summary>
/// Exposes lightweight application metrics.
/// </summary>
[ApiController]
[Route("metrics")] 
public class MetricsController : ControllerBase
{
    private readonly ISseClientRegistry _registry;

    public MetricsController(ISseClientRegistry registry) => _registry = registry;

    [HttpGet("sse")] 
    [ProducesResponseType(typeof(SseMetricsResponse), StatusCodes.Status200OK)]
    public ActionResult<SseMetricsResponse> GetSseMetrics() => Ok(new SseMetricsResponse(_registry.ActiveCount, DateTimeOffset.UtcNow));
}

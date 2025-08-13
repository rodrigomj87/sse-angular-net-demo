using Microsoft.AspNetCore.Mvc;
using SseDemo.OrdersService.Sse;

namespace SseDemo.OrdersService.Controllers;

/// <summary>
/// SSE streaming and diagnostics
/// </summary>
[ApiController]
[Route("sse")]
public class SseController : ControllerBase
{
    private readonly ISseClientRegistry _registry;
    private readonly ILogger<SseController> _logger;

    public SseController(ISseClientRegistry registry, ILogger<SseController> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    [HttpGet("stream")]
    public async Task Stream(CancellationToken ct)
    {
        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";
        var id = _registry.Register(Response, ct);
        await Response.WriteAsync(": connected \n\n");
        await Response.Body.FlushAsync(ct);
        try { await Task.Delay(Timeout.Infinite, ct); }
        catch (TaskCanceledException) { }
        finally { _registry.Remove(id); }
    }

    [HttpGet("clients")]
    [ProducesResponseType(typeof(SseClientsSnapshot), StatusCodes.Status200OK)]
    public ActionResult<SseClientsSnapshot> Clients() => Ok(_registry.GetSnapshot());
}

using Microsoft.AspNetCore.Mvc;

namespace SseDemo.OrdersService.Controllers;

/// <summary>
/// System / utility endpoints
/// </summary>
[ApiController]
[Route("")]
public class SystemController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { ok = true, service = "orders", ts = DateTimeOffset.UtcNow });
}

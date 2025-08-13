namespace SseDemo.OrdersService.Tracing;

public interface ITraceContextAccessor
{
    string? TraceId { get; set; }
}

namespace SseDemo.OrdersService.Tracing;

internal sealed class TraceContextAccessor : ITraceContextAccessor
{
    private static readonly AsyncLocal<string?> Holder = new();
    public string? TraceId
    {
        get => Holder.Value;
        set => Holder.Value = value;
    }
}

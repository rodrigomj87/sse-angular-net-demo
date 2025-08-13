using System.Collections.Concurrent;
using SseDemo.Domain.Abstractions;
using SseDemo.Domain.Entities;

namespace SseDemo.OrdersService.Repositories;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _store = new();

    public Task<Order> AddAsync(Order order, CancellationToken ct = default)
    {
        _store[order.Id] = order;
        return Task.FromResult(order);
    }

    public Task<Order?> GetAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task<IReadOnlyCollection<Order>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default)
    {
        var data = _store.Values
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToArray();
        return Task.FromResult((IReadOnlyCollection<Order>)data);
    }

    public Task<bool> UpdateAsync(Order order, CancellationToken ct = default)
    {
        _store[order.Id] = order;
        return Task.FromResult(true);
    }
}

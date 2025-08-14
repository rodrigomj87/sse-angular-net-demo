using SseDemo.Domain.Entities;

namespace SseDemo.Domain.Abstractions;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order, CancellationToken ct = default);
    Task<Order?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Order>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default);
    Task<bool> UpdateAsync(Order order, CancellationToken ct = default);
}

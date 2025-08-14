using SseDemo.Domain.Abstractions;
using SseDemo.Domain.Enums;
using SseDemo.Domain.Events;

namespace SseDemo.Domain.Entities;

public sealed class Order
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Guid Id { get; }
    public string Code { get; }
    public OrderStatus Status { get; private set; }
    public string CustomerName { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Order(Guid id, string code, string customerName, decimal totalAmount, DateTimeOffset createdAt)
    {
        Id = id;
        Code = code;
        Status = OrderStatus.Created;
        CustomerName = customerName;
        TotalAmount = totalAmount;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static Order Create(string customerName, decimal totalAmount, string? prefix = null)
    {
        if (string.IsNullOrWhiteSpace(customerName)) throw new ArgumentException("Customer name required", nameof(customerName));
        if (totalAmount < 0) throw new ArgumentOutOfRangeException(nameof(totalAmount));
        var id = Guid.NewGuid();
        var code = (prefix ?? "ORD") + "-" + id.ToString()[..8].ToUpperInvariant();
        var now = DateTimeOffset.UtcNow;
        var order = new Order(id, code, customerName.Trim(), decimal.Round(totalAmount, 2), now);
        order.AddEvent(new OrderCreatedDomainEvent(order.Id));
        return order;
    }

    public bool MarkPaid()
    {
        if (Status != OrderStatus.Created) return false;
        Status = OrderStatus.Paid;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddEvent(new OrderPaidDomainEvent(Id));
        return true;
    }

    public bool MarkFulfilled()
    {
        if (Status != OrderStatus.Paid) return false;
        Status = OrderStatus.Fulfilled;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddEvent(new OrderFulfilledDomainEvent(Id));
        return true;
    }

    public bool Cancel()
    {
        if (Status is OrderStatus.Fulfilled or OrderStatus.Cancelled) return false;
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddEvent(IDomainEvent evt) => _domainEvents.Add(evt);
}

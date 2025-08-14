// Copyright (c) SseDemo. All rights reserved.
#pragma warning disable SA1633, SA0001
namespace SseDemo.OrdersService.Tests;

using SseDemo.Domain.Entities;
using Xunit;

/// <summary>
/// Tests for <see cref="Order"/> entity state transitions.
/// </summary>
public class OrderEntityTests
{
    /// <summary>
    /// Creating an order sets status to New and copies basic fields.
    /// </summary>
    [Fact]
    public void Create_ShouldInitializeWithNewStatus()
    {
        var order = Order.Create("Customer", 10m);
        Assert.Equal("Created", order.Status.ToString());
        Assert.Equal(10m, order.TotalAmount);
        Assert.Equal("Customer", order.CustomerName);
    }

    /// <summary>
    /// MarkPaid transitions from New to Paid.
    /// </summary>
    [Fact]
    public void MarkPaid_FromNew_ShouldTransitionToPaid()
    {
        var order = Order.Create("C", 5m);
        var changed = order.MarkPaid();
        Assert.True(changed);
        Assert.Equal("Paid", order.Status.ToString());
    }

    /// <summary>
    /// Cannot fulfill directly from New; must remain New.
    /// </summary>
    [Fact]
    public void MarkFulfilled_WithoutPaid_ShouldFail()
    {
        var order = Order.Create("C", 5m);
        var changed = order.MarkFulfilled();
        Assert.False(changed);
        Assert.Equal("Created", order.Status.ToString());
    }
}

using Microsoft.EntityFrameworkCore;
using OnlineStore.Api.Common;
using OnlineStore.Api.Data;
using OnlineStore.Api.DTOs;
using OnlineStore.Api.Entities;
using OnlineStore.Api.Events;
using OnlineStore.Api.Interfaces;

namespace OnlineStore.Api.Services;

public sealed class OrderService
{
    private readonly AppDbContext _db;
    private readonly ICartRepository _carts;
    private readonly IOrderRepository _orders;
    private readonly IEventBus _bus;

    public OrderService(AppDbContext db, ICartRepository carts, IOrderRepository orders, IEventBus bus)
    {
        _db     = db;
        _carts  = carts;
        _orders = orders;
        _bus    = bus;
    }


    /// Places an order: validates stock, decrements stock, snapshots prices into
    /// OrderItems, and clears the cart. 
    public async Task<Result<OrderDto>> PlaceOrderAsync(int userId, CancellationToken ct = default)
    {
        var cart = await _carts.GetWithItemsAsync(userId, ct);
        if (cart is null || cart.Items.Count == 0)
            return Result<OrderDto>.Invalid("Cart is empty.");

        var productIds = cart.Items.Select(i => i.ProductId).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
            .ToDictionaryAsync(p => p.Id, ct);

        var order = new Order { UserId = userId, Status = OrderStatus.Pending };
        decimal total = 0m;

        foreach (var ci in cart.Items)
        {
            if (!products.TryGetValue(ci.ProductId, out var product))
                return Result<OrderDto>.NotFound($"Product {ci.ProductId} no longer available.");

            if (product.StockQuantity < ci.Quantity)
                return Result<OrderDto>.Conflict($"Insufficient stock for '{product.Name}' (have {product.StockQuantity}, need {ci.Quantity}).");

            product.StockQuantity -= ci.Quantity;
            product.LastModified  = DateTime.UtcNow;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                UnitPrice = product.Price,
                Quantity  = ci.Quantity
            });
            total += product.Price * ci.Quantity;
        }

        order.TotalPrice = total;
        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync(ct);

        _bus.Publish(new OrderPlacedEvent(order.Id, userId, total, DateTimeOffset.UtcNow));

        var fresh = await _orders.GetByIdAsync(order.Id, ct);
        return Result<OrderDto>.Ok(Map(fresh!));
    }

    public async Task<IReadOnlyList<OrderDto>> ListMineAsync(int userId, CancellationToken ct = default)
    {
        var orders = await _orders.ListByUserAsync(userId, ct);
        return orders.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> ListAllAsync(CancellationToken ct = default)
    {
        var orders = await _orders.ListAllAsync(ct);
        return orders.Select(Map).ToList();
    }

    // Returns null when the order does not exist or belongs to another non-admin user.
    public async Task<OrderDto?> GetAsync(int orderId, int userId, bool isAdmin, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(orderId, ct);
        if (order is null) return null;
        if (!isAdmin && order.UserId != userId) return null; // 404 instead of 403 to avoid leaking ids
        return Map(order);
    }

    public async Task<Result<OrderDto>> UpdateStatusAsync(int orderId, string newStatus, CancellationToken ct = default)
    {
        if (!OrderStatus.All.Contains(newStatus))
            return Result<OrderDto>.Invalid($"Invalid status '{newStatus}'. Allowed: {string.Join(", ", OrderStatus.All)}.");

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
            return Result<OrderDto>.NotFound($"Order {orderId} not found.");

        var old = order.Status;
        order.Status = newStatus;
        await _db.SaveChangesAsync(ct);

        _bus.Publish(new OrderStatusChangedEvent(order.Id, old, newStatus, DateTimeOffset.UtcNow));

        var fresh = await _orders.GetByIdAsync(orderId, ct);
        return Result<OrderDto>.Ok(Map(fresh!));
    }

    private static OrderDto Map(Order o)
    {
        var items = o.Items.Select(i => new OrderItemDto(
            i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.UnitPrice,
            i.Quantity,
            i.LineTotal)).ToList();
        return new OrderDto(o.Id, o.UserId, o.Status, o.TotalPrice, o.CreatedAt, items);
    }
}


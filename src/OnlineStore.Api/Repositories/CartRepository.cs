using Microsoft.EntityFrameworkCore;
using OnlineStore.Api.Data;
using OnlineStore.Api.Entities;
using OnlineStore.Api.Interfaces;

namespace OnlineStore.Api.Repositories;

public sealed class CartRepository : ICartRepository
{
    private readonly AppDbContext _db;
    public CartRepository(AppDbContext db) => _db = db;

    public async Task<ShoppingCart> GetOrCreateAsync(int userId, CancellationToken ct = default)
    {
        var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (cart is not null) return cart;

        cart = new ShoppingCart { UserId = userId };
        _db.ShoppingCarts.Add(cart);
        await _db.SaveChangesAsync(ct);
        return cart;
    }

    public Task<ShoppingCart?> GetWithItemsAsync(int userId, CancellationToken ct = default) =>
        _db.ShoppingCarts.AsNoTracking()
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public async Task AddOrUpdateItemAsync(int cartId, int productId, int quantity, decimal unitPrice, CancellationToken ct = default)
    {
        var existing = await _db.CartItems
            .FirstOrDefaultAsync(i => i.ShoppingCartId == cartId && i.ProductId == productId, ct);

        if (existing is null)
        {
            _db.CartItems.Add(new CartItem
            {
                ShoppingCartId = cartId,
                ProductId      = productId,
                Quantity       = quantity,
                UnitPrice      = unitPrice
            });
        }
        else
        {
            existing.Quantity  = quantity;
            existing.UnitPrice = unitPrice;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveItemAsync(int cartId, int productId, CancellationToken ct = default)
    {
        var existing = await _db.CartItems
            .FirstOrDefaultAsync(i => i.ShoppingCartId == cartId && i.ProductId == productId, ct);
        if (existing is null) return;
        _db.CartItems.Remove(existing);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ClearAsync(int cartId, CancellationToken ct = default)
    {
        var items = await _db.CartItems.Where(i => i.ShoppingCartId == cartId).ToListAsync(ct);
        if (items.Count == 0) return;
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync(ct);
    }
}

using OnlineStore.Api.Common;
using OnlineStore.Api.DTOs;
using OnlineStore.Api.Entities;
using OnlineStore.Api.Interfaces;

namespace OnlineStore.Api.Services;

public sealed class CartService
{
    private readonly ICartRepository _carts;
    private readonly IProductRepository _products;

    public CartService(ICartRepository carts, IProductRepository products)
    {
        _carts    = carts;
        _products = products;
    }

    public async Task<CartDto> GetAsync(int userId, CancellationToken ct = default)
    {
        // GetOrCreate ensures the cart exists, so the read below is never null.
        await _carts.GetOrCreateAsync(userId, ct);
        var cart = await _carts.GetWithItemsAsync(userId, ct);
        return Map(cart!);
    }

    public async Task<Result<CartDto>> AddItemAsync(int userId, AddCartItemRequest req, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(req.ProductId, ct);
        if (product is null)
            return Result<CartDto>.NotFound($"Product {req.ProductId} not found.");

        if (product.StockQuantity < req.Quantity)
            return Result<CartDto>.Conflict($"Only {product.StockQuantity} units of '{product.Name}' available.");

        var cart = await _carts.GetOrCreateAsync(userId, ct);
        await _carts.AddOrUpdateItemAsync(cart.Id, product.Id, req.Quantity, product.Price, ct);

        return Result<CartDto>.Ok(await GetAsync(userId, ct));
    }

    public async Task<CartDto> RemoveItemAsync(int userId, int productId, CancellationToken ct = default)
    {
        var cart = await _carts.GetOrCreateAsync(userId, ct);
        await _carts.RemoveItemAsync(cart.Id, productId, ct);
        return await GetAsync(userId, ct);
    }

    private static CartDto Map(ShoppingCart cart)
    {
        var items = cart.Items.Select(i => new CartItemDto(
            i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.Quantity,
            i.UnitPrice,
            i.Quantity * i.UnitPrice)).ToList();

        return new CartDto(cart.Id, items, items.Sum(i => i.LineTotal));
    }
}

